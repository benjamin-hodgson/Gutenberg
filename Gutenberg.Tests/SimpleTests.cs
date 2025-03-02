using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

// test documents without grouping or alternatives
public class SimpleTests
{
    [Fact]
    public void TestEmpty()
    {
        TestDocument("", Doc.Empty);
    }

    [Fact]
    public void TestFromString()
    {
        TestDocument("a", Doc.FromString("a"));
        TestDocument("abc", Doc.FromString("abc"));
        TestDocument("\n", Doc.FromString("\n"));
        TestDocument("abc\ndef", Doc.FromString("abc\ndef"));
    }

    [Fact]
    public void TestLine()
    {
        TestDocument("\n", Doc.LineBreak);
        TestDocument("\n", Doc.ZeroWidthLineBreak);
        TestDocument("\n", Doc.LineBreakOr("x"));
    }

    [Fact]
    public void TestAppend()
    {
        TestDocument("abcdef", Doc.FromString("abc").Append("def"));
        TestDocument("abc\ndef", Doc.FromString("abc").Append(Doc.LineBreak).Append("def"));
    }

    [Fact]
    public void TestConcat()
    {
        TestDocument("abcdefghi", Doc.Concat("abc", "def", "ghi"));
        TestDocument(new string('a', 100), Doc.Concat(Enumerable.Repeat(Doc.FromString("a"), 100)));
    }

    [Fact]
    public void TestBetween()
    {
        TestDocument("[abc]", Doc.FromString("abc").Between("[", "]"));
    }

    [Fact]
    public void TestSeparated()
    {
        TestDocument(
            "abc,def,ghi",
            new Doc[] { "abc", "def", "ghi" }
                .Separated(",")
        );
        TestDocument(
            "abc;def;ghi;",
            new Doc[] { "abc", "def", "ghi" }
                .SeparatedAndTerminated(";")
        );
    }

    [Fact]
    public void TestNested()
    {
        TestDocument(
            "\n  abc",
            Doc.LineBreak
                .Append("abc")
                .Nested(2)
        );
        TestDocument(
            "\n  abc",
            Doc.LineBreak
                .Append("abc")
                .Nested(),
            LayoutOptions.Default with { DefaultNesting = 2 }
        );
        TestDocument(
            "abc\n  def",
            Doc.Concat("abc", Doc.LineBreak, "def")
                .Nested(2)
        );
        TestDocument(
            "abc\n  def",
            Doc.Concat("abc", Doc.LineBreak, "def")
                .Nested(),
            LayoutOptions.Default with { DefaultNesting = 2 }
        );
    }

    [Fact]
    public void StripTrailingWhitespace()
    {
        TestDocument(
            "\n",
            Doc.HardLineBreak.Append(Doc.Empty).Nested(2)
        );

        // debatable: spaces appearing in FromString literals are
        // counted as text by LineContainsTextAfter.
        // https://github.com/benjamin-hodgson/Gutenberg/issues/11
        TestDocument(
            "abc\n\n  def  \n",
            Doc.Concat("abc", Doc.LineBreak, Doc.LineBreak, "def  ", Doc.LineBreak)
                .Nested(2)
        );
    }

    [Fact]
    public void DisableStripTrailingWhitespace()
    {
        TestDocument(
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
    public void TestPlainTextDocumentRendererWithValueTypeAnnotation()
    {
        var doc = Document<int>
            .FromString("abc")
            .Annotated(2);

        // TestDocument calls ToString, which uses a PlainTextDocumentRenderer
        TestDocument("abc", doc);
    }
}
