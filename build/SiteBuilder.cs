using System.Collections.Concurrent;

internal partial class SiteBuilder
{
	public const string ArticlesFolder = "articles";
	public const string OutputFolder = "output";

	public const string MainTitle = "JJ";
	public const string IndexQuote = "Easy to Say, Hard to Do";
	public const string H1Title = "JJ's blog";
	public const string ArticleTitleSuffix = H1Title;

	public readonly Uri? host;
	private readonly ConcurrentBag<Article> articles = new();

	public SiteBuilder(Uri? host)
	{
		this.host = host;
	}

	public Task PostArticlesBuild()
	{
		return BuildIndexPage();
	}

	private static Task WriteHeader(TextWriter output, ReadOnlySpan<char> title, string suffix)
	{
		return output.WriteAsync($"""
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="utf-8">
			<title>{title} | {suffix}</title>
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
