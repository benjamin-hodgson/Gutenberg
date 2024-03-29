using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

// test documents without grouping or alternatives
public class SimpleTests
{
    [Fact]
    public async Task TestEmpty()
    {
        await TestDocument("", Doc.Empty);
    }

    [Fact]
    public async Task TestFromString()
    {
        await TestDocument("a", Doc.FromString("a"));
        await TestDocument("abc", Doc.FromString("abc"));
        await TestDocument("\n", Doc.FromString("\n"));
        await TestDocument("abc\ndef", Doc.FromString("abc\ndef"));
    }

    [Fact]
    public async Task TestLine()
    {
        await TestDocument("\n", Doc.LineBreak);
        await TestDocument("\n", Doc.ZeroWidthLineBreak);
        await TestDocument("\n", Doc.LineBreakOr("x"));
    }

    [Fact]
    public async Task TestAppend()
    {
        await TestDocument("abcdef", Doc.FromString("abc").Append("def"));
        await TestDocument("abc\ndef", Doc.FromString("abc").Append(Doc.LineBreak).Append("def"));
    }

    [Fact]
    public async Task TestConcat()
    {
        await TestDocument("abcdefghi", Doc.Concat("abc", "def", "ghi"));
    }

    [Fact]
    public async Task TestBetween()
    {
        await TestDocument("[abc]", Doc.FromString("abc").Between("[", "]"));
    }

    [Fact]
    public async Task TestSeparated()
    {
        await TestDocument(
            "abc,def,ghi",
            new Doc[] { "abc", "def", "ghi" }
                .Separated(",")
        );
        await TestDocument(
            "abc;def;ghi;",
            new Doc[] { "abc", "def", "ghi" }
                .SeparatedAndTerminated(";")
        );
    }

    [Fact]
    public async Task TestNested()
    {
        await TestDocument(
            "\n  abc",
            Doc.LineBreak
                .Append("abc")
                .Nested(2)
        );
        await TestDocument(
            "\n  abc",
            Doc.LineBreak
                .Append("abc")
                .Nested(),
            LayoutOptions.Default with { DefaultNesting = 2 }
        );
        await TestDocument(
            "abc\n  def",
            Doc.Concat("abc", Doc.LineBreak, "def")
                .Nested(2)
        );
        await TestDocument(
            "abc\n  def",
            Doc.Concat("abc", Doc.LineBreak, "def")
                .Nested(),
            LayoutOptions.Default with { DefaultNesting = 2 }
        );
    }

    [Fact]
    public async Task StripTrailingWhitespace()
    {
        // spaces in "def" literal are counted as text by FromString.
        // https://github.com/benjamin-hodgson/Gutenberg/issues/11
        await TestDocument(
            "abc\n\n  def  \n",
            Doc.Concat("abc", Doc.LineBreak, Doc.LineBreak, "def  ", Doc.LineBreak)
                .Nested(2)
        );
    }

    [Fact]
    public async Task DisableStripTrailingWhitespace()
    {
        await TestDocument(
            "abc\n  \n  def  \n  ",
            Doc.Concat("abc", Doc.LineBreak, Doc.LineBreak, "def  ", Doc.LineBreak)
                .Nested(2),
            LayoutOptions.Default with { StripTrailingWhitespace = false }
        );
    }

    [Fact]
    public async Task TestAnnotations()
    {
        var doc = Document<int>
            .FromString("abc")
            .Annotated(2);

        Assert.Equal("PUSH(2)abcPOP", await ObserveAnnotations(doc));
        Assert.Equal("PUSH(3)abcPOP", await ObserveAnnotations(doc.MapAnnotations(x => x + 1)));
        Assert.Equal("PUSH(3)abcPOP", await ObserveAnnotations(doc, x => x + 1));
    }

    [Fact]
    public async Task TestPlainTextDocumentRendererWithValueTypeAnnotation()
    {
        var doc = Document<int>
            .FromString("abc")
            .Annotated(2);

        // TestDocument calls ToString, which uses a PlainTextDocumentRenderer
        await TestDocument("abc", doc);
    }
}
