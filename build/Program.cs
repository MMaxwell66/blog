class Program
{
	private const string ArticlesFolder = "articles";
	private const string OutputFolder = "output";

	/// <summary>
	/// My blog's build system.
	/// </summary>
	/// <param name="force">force generation</param>
	static void Main(bool force = false)
	{
		var articles = new DirectoryInfo(ArticlesFolder);
		if (!articles.Exists)
			throw new DirectoryNotFoundException($"Articles directory not found: {articles.FullName}");

		if (force && Directory.Exists(OutputFolder))
			Directory.Delete(OutputFolder, true);
		var output = Directory.CreateDirectory(OutputFolder);

		static void CopyDirectory(DirectoryInfo dir, string dest, bool force)
		{
			Directory.CreateDirectory(dest);
			foreach (var subDir in dir.GetDirectories())
				CopyDirectory(subDir, Path.Combine(dest, subDir.Name), force);

			foreach (var file in dir.GetFiles())
			{
				var destFile = Path.Combine(dest, file.Name);
				var destFileInfo = new FileInfo(destFile);
				if (!force && destFileInfo.Exists && destFileInfo.LastWriteTimeUtc >= file.LastWriteTimeUtc)
					continue;
				file.CopyTo(destFile, true);
			}
		}
		CopyDirectory(articles, OutputFolder, force);
	}
}
