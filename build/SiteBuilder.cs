using System.Text;
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

	public async Task BuildMarkdown(FileStream output, string content, string title)
	{
		await WriteHeader(output, title);
		_ = Markdown.ToHtml(content, new StreamWriter(output));
		await WriteFooter(output);
	}

	private static readonly byte[] HeaderBytes1 = Encoding.ASCII.GetBytes("""
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="utf-8" />
			<title>
		""");
	private static readonly byte[] HeaderBytes2 = Encoding.ASCII.GetBytes("""
		</title>
		</head>
		<body>

		""");

	private static async Task WriteHeader(FileStream output, string title)
	{
		// TODO(JJ): find the appropriate value for title
		await output.WriteAsync(HeaderBytes1);
		await output.WriteAsync(Encoding.UTF8.GetBytes(title));
		await output.WriteAsync(HeaderBytes2);
	}

	private static readonly byte[] FooterBytes = Encoding.ASCII.GetBytes("</body>\n</html>");

	private static async Task WriteFooter(FileStream output)
	{
		await output.WriteAsync(FooterBytes);
	}
}
