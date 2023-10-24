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
			<h2><a href="{article.UrlPath}" rel="bookmark">{article.Title}</a></h2>
			<!-- TODO: post time & updated time -->
			<!-- <div><span></span></div> -->
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
