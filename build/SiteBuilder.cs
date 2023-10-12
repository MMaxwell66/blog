using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

internal class SiteBuilder
{
	private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
		.UseYamlFrontMatter()
		.Build();
	public async Task BuildMarkdown(TextWriter output, string content)
	{
		var document = Markdown.Parse(content, pipeline);

		var title = GetTitle(document);
		if (title.IsEmpty)
			title = "Welcome!".AsMemory();

		await WriteHeader(output, title);
		Markdown.ToHtml(document, output, pipeline);
		await WriteFooter(output);
	}

	private static ReadOnlyMemory<char> GetTitle(MarkdownDocument document)
	{
		var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
		if (yamlBlock is null)
			return default;
		var yaml = Utils.ParseYaml(yamlBlock.Lines);
		return yaml.First(kv => kv.Item1.AsSpan().SequenceEqual("title")).Item2.AsMemory();
	}

	private static Task WriteHeader(TextWriter output, ReadOnlyMemory<char> title)
	{
		return output.WriteAsync($"""
		<!DOCTYPE html>
		<html lang="en">
		<head>
			<meta charset="utf-8">
			<title>{title.Span}</title>
		</head>
		<body>

		""");
	}

	private static Task WriteFooter(TextWriter output)
	{
		return output.WriteAsync("</body>\n</html>");
	}
}
