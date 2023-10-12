using Markdig;

internal class SiteBuilder
{
	private static readonly string IndexMd = $"{Path.DirectorySeparatorChar}index.md";
	public static string GetTitle(string srcFileRel)
	{
		if (srcFileRel.EndsWith(IndexMd))
			return Path.GetDirectoryName(srcFileRel)!;
		else if (srcFileRel == "index.md")
			return "Welcome!";
		else
			return Path.GetFileNameWithoutExtension(srcFileRel);
	}

	public async Task BuildMarkdown(TextWriter output, string content, string title)
	{
		await WriteHeader(output, title);
		_ = Markdown.ToHtml(content, output);
		await WriteFooter(output);
	}

	private static async Task WriteHeader(TextWriter output, string title)
	{
		// TODO(JJ): find the appropriate value for title
		await output.WriteAsync($"""
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="utf-8">
			<title>{title}</title>
		</head>
		<body>

		""");
	}

	private static async Task WriteFooter(TextWriter output)
	{
		await output.WriteAsync("</body>\n</html>");
	}
}
