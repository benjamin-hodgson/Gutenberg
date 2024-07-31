using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

public class LayoutModeTests
{
    [Fact]
    public void TestLayoutModeSimple()
    {
        var doc = Doc.Concat("abc", Doc.LineBreak, "def")
            .Nested(2)
            .Grouped();

        TestDocument(
            "abc\ndef",
            doc,
            LayoutOptions.Default with { LayoutMode = LayoutMode.Simple }
        );
    }

    [Fact]
    public void TestLayoutModeSmart()
    {
        // the last line will overflow (75 + 19 = 94),
        // even though it wouldn't overflow if the
        // layout algorithm had taken one of the
        // LineBreakHints in the first line
        var alignedText = new[]
        {
            "and now",
            "some aligned",
            "text",
            "with an unexpectedly long line later on in the block (after the first line)"
        };
        var alignedDoc = alignedText
            .Select(Doc.FromString)
            .Separated(Doc.LineBreak)
            .Aligned();

        // block should still be aligned with the end of 'words'
        var expected = "some opening\nwords"
            + string.Join("\n     ", alignedText);

        TestDocument(
            expected,
            Doc.Concat(
                Doc.Reflow("some opening words"),
                alignedDoc
            ),
            LayoutOptions.Default with { LayoutMode = LayoutMode.Smart }
        );
    }

    [Fact]
    public void TestLayoutModeSmart_NoOverflow()
    {
        var alignedDoc = new Doc[]
            {
                "and now",
                "some aligned",
                "text",
            }
            .Separated(Doc.LineBreak)
            .Aligned();

        var doc = Doc.Concat(
            Doc.Reflow("some opening words"),
            alignedDoc
        );

        // should agree with default mode
        Assert.Equal(
            doc.ToString(),
            doc.ToString(LayoutOptions.Default with { LayoutMode = LayoutMode.Smart })
        );
    }
}
