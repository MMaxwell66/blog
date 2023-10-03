using System.Text;
using Markdig;

internal class SiteBuilder
{
	public async Task BuildMarkdown(string content, FileStream output)
	{
		await WriteHeader(output);
		_ = Markdown.ToHtml(content, new StreamWriter(output));
		await WriteFooter(output);
	}

	private static readonly byte[] HeaderBytes = Encoding.ASCII.GetBytes("""
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="utf-8" />
			<title>Welcome!</title>
		</head>
		<body>

		""");

	private static async Task WriteHeader(FileStream output)
	{
		// TODO(JJ): find the appropriate value for title
		await output.WriteAsync(HeaderBytes);
	}

	private static readonly byte[] FooterBytes = Encoding.ASCII.GetBytes("</body>\n</html>");

	private static async Task WriteFooter(FileStream output)
	{
		await output.WriteAsync(FooterBytes);
	}
}
