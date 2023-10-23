using System.Diagnostics;
using System.IO.Enumeration;

class Program
{
	/// <summary>
	/// My blog's build system.
	/// </summary>
	/// <param name="host">host name for the blog</param>
	/// <param name="force">force generation</param>
	/// <param name="verbosity"></param>
	static async Task Main(
		Uri? host = null,
		bool force = false,
		Verbosity verbosity = Verbosity.Normal)
	{
		Log.level = verbosity;

		if (!Directory.Exists(SiteBuilder.ArticlesFolder))
			throw new DirectoryNotFoundException($"Articles directory not found: {Path.GetFullPath(SiteBuilder.ArticlesFolder)}");

		if (force && Directory.Exists(SiteBuilder.OutputFolder))
			Directory.Delete(SiteBuilder.OutputFolder, true);
		Directory.CreateDirectory(SiteBuilder.OutputFolder);

		Log.WriteLine($"Building for '{host?.ToString() ?? "localhost"}' ...");
		Log.WriteLine($"Building article folder: {Path.GetFullPath(SiteBuilder.ArticlesFolder)}");
		var siteBuilder = new SiteBuilder(host);

		await Parallel.ForEachAsync(
			new FileSystemEnumerable<(string, bool)>(
				SiteBuilder.ArticlesFolder,
				(ref FileSystemEntry entry) =>
				{
					var folder  = entry.Directory[entry.RootDirectory.Length..];
					if (folder.Length > 0) folder = folder[1..];
					return (Path.Join(folder, entry.FileName), entry.IsDirectory);
				},
				new() { RecurseSubdirectories = true }),
			new ParallelOptions(){ MaxDegreeOfParallelism = Debugger.IsAttached ? 1 : -1 },
			async (item, _) =>
		{
			var (relativePath, isDirectory) = item;
			var srcFile = Path.Join(SiteBuilder.ArticlesFolder, relativePath);
			var destFile = Path.Join(SiteBuilder.OutputFolder, isDirectory ? relativePath : Path.ChangeExtension(relativePath, ".html"));
			if (isDirectory) {
				Directory.CreateDirectory(destFile);
				return;
			}

			if (Path.GetExtension(srcFile) != ".md")
				throw new ArgumentOutOfRangeException($"[{relativePath}]: only support markdown file");

			// skip file without change
			var destFileInfo = new FileInfo(destFile);
			if (!force && destFileInfo.Exists && destFileInfo.LastWriteTimeUtc >= File.GetLastWriteTimeUtc(srcFile))
				return;

			// TODO: as we parallel, need an identifier to distinct iter when we have more log
			Log.DiagWriteLine($"Building file: {relativePath}");

			var input = await File.ReadAllTextAsync(srcFile);
			using var output = new StreamWriter(destFile);

			var destUrlPath = Path.ChangeExtension(relativePath, null);
			await siteBuilder.BuildArticle(destUrlPath, output, input);
		});

		await siteBuilder.PostArticlesBuild();
	}
}
