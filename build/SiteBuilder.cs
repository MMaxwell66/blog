internal partial class SiteBuilder
{
	public const string ArticlesFolder = "articles";
	public const string OutputFolder = "output";

	public Task PostArticlesBuild()
	{
		return BuildIndexPage();
	}

	private static Task WriteHeader(TextWriter output, ReadOnlySpan<char> title)
	{
		return output.WriteAsync($"""
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="utf-8">
			<title>{title}</title>
		</head>
		<body>

		""");
	}

	private static Task WriteFooter(TextWriter output)
	{
		return output.WriteAsync("</body>\n</html>");
	}
}
