internal partial class SiteBuilder
{
	private static Task<(string abbr, string full)> HeadHash = GetHeadHash();

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

		var (headAbbr, headFull) = await HeadHash;
		await output.WriteAsync(
$"""
	<nav>
		<!-- TODO: pagination -->
	</nav>
</main>
<footer>
	<div>
		<span>Commit <a href="{repoUrl}/commits/{headFull}" target="_blank">{headAbbr}</a> on <a href="{repoUrl}" target="_blank">GitHub</a></span> | <span><a href="https://creativecommons.org/licenses/by-nc-sa/4.0/" target="_blank">CC BY-NC-SA 4.0</a></span><br>
		<span>Theme based on <a href="http://www.matrix67.com/blog/wp-content/themes/matrix67/style.css" target="_blank">Mathix67</a></span>
	</div>
</footer>

""");

		await WriteFooter(output);
	}

	private static async Task<(string abbr, string full)> GetHeadHash()
	{
		var hashes = (await Utils.RunCommandAndGetOutput("git", "log -1 --pretty=\"format:%h %H\"")).Split(' ');
		if (hashes.Length != 2)
			throw new ApplicationException("Invalid git head.");
		return (hashes[0], hashes[1]);
	}
}
