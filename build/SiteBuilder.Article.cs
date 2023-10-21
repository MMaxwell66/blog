using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

internal partial class SiteBuilder
{
	private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
		.UseYamlFrontMatter()
		.Build();
	public async Task BuildArticle(TextWriter output, string content)
	{
		var document = Markdown.Parse(content, pipeline);

		var title = GetTitle(document);
		if (title.IsEmpty)
			title = "Welcome!".AsMemory();

		await WriteHeader(output, title.Span);
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
}
