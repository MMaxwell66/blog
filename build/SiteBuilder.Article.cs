using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

internal partial class SiteBuilder
{
	private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
		.UseYamlFrontMatter()
		.Use<ReadMoreExtension>()
		.Build();
	public async Task BuildArticle(string urlPath, TextWriter output, string content)
	{
		var document = Markdown.Parse(content, pipeline);

		var title = GetTitle(document);
		if (title.IsEmpty)
			throw new ArgumentException($"[{urlPath}]: do not have title");

		await WriteHeader(output, title.Span, ArticleTitleSuffix);
		Markdown.ToHtml(document, output, pipeline);
		await WriteFooter(output);

		var article = new Article() { UrlPath = urlPath, Title = title.ToString() };

		var trimmed = document.TrimReadMore();
		article.ReadLessText = Markdown.ToHtml(document, pipeline);
		if (trimmed)
			article.ReadLessText += $"<p><a href={urlPath}>Read more...</a></p>";

		articles.Add(article);
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
