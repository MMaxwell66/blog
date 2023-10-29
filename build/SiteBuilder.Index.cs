internal partial class SiteBuilder
{
	public async Task BuildIndexPage()
	{
		using var output = new StreamWriter(Path.Join(OutputFolder, "index.html"));

		await WriteHeader(output, MainTitle, IndexQuote);

		await output.WriteAsync(
$"""
<header><h1><a href="{host}">{H1Title}</a></h1></header>
<main>

""");

		// TODO: pagination
		// ThenBy is just to make sure the order is consistence
		foreach (var article in articles.OrderByDescending(article => article.PostTime).ThenBy(article => article.UrlPath))
		{
			await output.WriteAsync(
$"""
	<article>
		<header>
			<h2 class="title"><a href="{article.UrlPath}" rel="bookmark">{article.Title}</a></h2>
			<div class="time">
				<time datetime="{article.PostTime:yyyy-MM-ddTHH:mm:ssK}">{article.PostTime:yyyy/MM/dd}</time>

""");
			if (article.EditTime != article.PostTime)
				// fix to live branch now, no support for PR review now.
				await output.WriteAsync(
$"""
				<span><a href="{repoUrl}/commits/{branch}/{article.SrcPath}">• edited</a></span>

""");

			await output.WriteAsync(
"""
			</div>
		</header>
		<div class="content">

""");
			await output.WriteAsync(article.ReadLessText);
			await output.WriteAsync(
"""
		</div>
	</article>

""");
		}

		await output.WriteAsync(
"""
	<nav>
		<!-- TODO: pagination -->
	</nav>
</main>
<footer>
	<!-- TODO -->
</footer>

""");

		await WriteFooter(output);
	}
}
