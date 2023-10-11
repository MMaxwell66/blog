using System.Diagnostics;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

public class ReadMoreExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		pipeline.BlockParsers.AddIfNotAlready(new ReadMoreBlockParser());
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
			htmlRenderer.ObjectRenderers.AddIfNotAlready<ReadMoreHtmlRenderer>();
	}
}

public class ReadMoreBlock : LeafBlock
{
	public ReadMoreBlock(BlockParser parser) : base(parser)
	{
	}
}

public class ReadMoreBlockParser : BlockParser
{
	public ReadMoreBlockParser()
	{
		OpeningCharacters = new[] { '-' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
			return BlockState.None;

		var line = processor.Line;
		var cnt = 1;
		while (line.NextChar() == '-')
			cnt++;
		if (cnt < 3 || !line.CurrentChar.IsWhitespace())
			return BlockState.None;

		line.TrimStart();
		if (!line.MatchLowercase("more"))
			return BlockState.None;

		line.Start += "more".Length;
		if (!line.CurrentChar.IsWhitespace())
			return BlockState.None;

		line.Trim(); // also trim whitespace at end
		cnt = 0;
		while (line.CurrentChar == '-')
		{
			cnt++;
			line.SkipChar();
		}

		if (cnt < 3 || !line.IsEmpty)
			return BlockState.None;

		processor.NewBlocks.Push(new ReadMoreBlock(this)
		{
			Column = processor.Column,
			Span = new SourceSpan(processor.Start, line.End),
		});
		return BlockState.BreakDiscard;
	}
}

public class ReadMoreHtmlRenderer : HtmlObjectRenderer<ReadMoreBlock>
{
	protected override void Write(HtmlRenderer renderer, ReadMoreBlock obj)
	{
		renderer.EnsureLine();
		renderer.Write("""<span id="read-more"></span>""");
	}
}

public static class ReadMoreExtensions
{
	public static bool TrimReadMore(this MarkdownDocument document)
	{
		var enumerator = document.Descendants<ReadMoreBlock>().GetEnumerator();
		if (!enumerator.MoveNext())
			return false;
		var readMoreBlock = enumerator.Current;
		if (enumerator.MoveNext())
			throw new ArgumentException($"More than one read more block found ({enumerator.Current.ToPositionText()}).");

		var block = readMoreBlock as Block;
		Debug.Assert(block.Parent != null);
		var idx = block.Parent.IndexOf(block);
		if (idx != 0)
		{
			block = block.Parent[idx - 1];
		}
		else
		{
			block = block.Parent;
			((ContainerBlock)block).Clear();
		}

		while (block.Parent != null)
		{
			while (block.Parent.LastChild != block)
				block.Parent.RemoveAt(block.Parent.Count - 1);
			block = block.Parent;
		}

		return true;
	}
}
