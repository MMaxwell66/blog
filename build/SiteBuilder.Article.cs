using System.Globalization;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

internal partial class SiteBuilder
{
	private static DateTime buildTime = DateTime.UtcNow;

	private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
		.UseYamlFrontMatter()
		.Use<ReadMoreExtension>()
		.Build();
	public async Task BuildArticle(string urlPath, string destFilePath, string srcFilePath)
	{
		var content = await File.ReadAllTextAsync(srcFilePath);
		using var output = new StreamWriter(destFilePath);

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

		// Perf: this git log command might not be efficient as it goes through commit history multiple times.
		// PostTime
		var time = await Utils.RunCommandAndGetOutput("git", $"log --diff-filter=A --follow -1 --pretty=format:%cI -- {srcFilePath}");
		if (Utils.IsCI || !string.IsNullOrEmpty(time))
			article.PostTime = DateTimeOffset.ParseExact(time, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture).UtcDateTime;
		else // not committed yet
			article.PostTime = buildTime;

		// EditTime
		time = await Utils.RunCommandAndGetOutput("git", $"log --diff-filter=M --follow -1 --pretty=format:%cI -- {srcFilePath}");
		article.EditTime = article.PostTime;
		if (!string.IsNullOrEmpty(time))
			article.EditTime = DateTimeOffset.ParseExact(time, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture).UtcDateTime;

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
