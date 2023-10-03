using System.IO.Enumeration;

class Program
{
	private const string ArticlesFolder = "articles";
	private const string OutputFolder = "output";

	/// <summary>
	/// My blog's build system.
	/// </summary>
	/// <param name="force">force generation</param>
	/// <param name="verbosity"></param>
	static void Main(bool force = false, Verbosity verbosity = Verbosity.Normal)
	{
		Log.level = verbosity;

		if (!Directory.Exists(ArticlesFolder))
			throw new DirectoryNotFoundException($"Articles directory not found: {Path.GetFullPath(ArticlesFolder)}");

		if (force && Directory.Exists(OutputFolder))
			Directory.Delete(OutputFolder, true);
		Directory.CreateDirectory(OutputFolder);

		Log.WriteLine($"Building article folder: {Path.GetFullPath(ArticlesFolder)}");

		foreach (var (file, isDirectory) in
			new FileSystemEnumerable<(string, bool)>(
				ArticlesFolder,
				(ref FileSystemEntry entry) =>
				{
					var folder  = entry.Directory[entry.RootDirectory.Length..];
					if (folder.Length > 0) folder = folder[1..];
					return (Path.Join(folder, entry.FileName), entry.IsDirectory);
				},
				new() { RecurseSubdirectories = true }))
		{
			var srcFile = Path.Join(ArticlesFolder, file);
			var destFile = Path.Join(OutputFolder, file);
			if (isDirectory) {
				Directory.CreateDirectory(destFile);
				continue;
			}

			// skip file without change
			var destFileInfo = new FileInfo(destFile);
			if (!force && destFileInfo.Exists && destFileInfo.LastWriteTimeUtc >= File.GetLastWriteTimeUtc(srcFile))
				continue;

			Log.DiagWriteLine($"Building file: {file}");
			File.Copy(srcFile, destFile, true);
		}
	}
}
