using System.Collections.Concurrent;

internal partial class SiteBuilder
{
	public const string ArticlesFolder = "articles";
	public const string OutputFolder = "output";

	public const string MainTitle = "JJ";
	public const string IndexQuote = "Easy to Say, Hard to Do";
	public const string H1Title = "JJ's blog";
	public const string ArticleTitleSuffix = H1Title;

	private readonly Uri? host;
	private readonly bool force;
	private readonly ConcurrentBag<Article> articles = new();

	public SiteBuilder(Uri? host, bool force)
	{
		this.host = host;
		this.force = force;
	}

	public Task PostArticlesBuild()
	{
		var tasks = new List<Task>()
		{
			BuildIndexPage(),
			CopyAssets(),
		};

		return Task.WhenAll(tasks);
	}

	private async Task CopyAssets()
	{
		var cssRelPath = Path.Join("assets", "style.css");
		var cssInputInfo = new FileInfo(cssRelPath);
		if (!cssInputInfo.Exists)
			throw new ArgumentException("Missing style.css CSS file.");
		
		var cssOutputPath = Path.Join(OutputFolder, cssRelPath);
		var cssOutputInfo = new FileInfo(cssOutputPath);
		if (!force && cssOutputInfo.Exists && cssOutputInfo.LastWriteTimeUtc >= cssInputInfo.LastWriteTimeUtc)
		{
			return;
		}
		else if (force)
		{
			// Not elegant, but easy to do now.
			var data = await new HttpClient().GetStringAsync("https://necolas.github.io/normalize.css/latest/normalize.css");
			if (!data.Contains("v8.0.1"))
				throw new ArgumentException("normalize.css is out of date");
		}

		if (!Directory.Exists(Path.Join(OutputFolder, "assets")))
			Directory.CreateDirectory(Path.Join(OutputFolder, "assets"));

		cssInputInfo.CopyTo(cssOutputPath, true);
	}

	private static Task WriteHeader(TextWriter output, ReadOnlySpan<char> title, string suffix)
	{
		return output.WriteAsync($"""
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="utf-8">
			<title>{title} | {suffix}</title>
			<meta name="viewport" content="width=device-width, initial-scale=1">
			<link rel="stylesheet" href="assets/style.css">
		</head>
		<body>

		""");
	}

	private static Task WriteFooter(TextWriter output)
	{
		return output.WriteAsync("</body>\n</html>");
	}

	private struct Article
	{
		public string UrlPath;
		public string Title;
		public DateTime PostTime;
		public string ReadLessText;
	}
}
