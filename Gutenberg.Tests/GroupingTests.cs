using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

public class GroupingTests
{
    [Fact]
    public async Task LineGroupedWhenFits()
    {
        static Task DoTest(string expected, Doc doc)
            => TestDocument(expected, doc.Grouped());

        await DoTest(
            "abc def",
            "abc\ndef"
        );
        await DoTest(
            "abc def",
            Doc.FromString("abc") + Doc.LineBreak + "def"
        );
        await DoTest(
            "abcdef",
            Doc.FromString("abc") + Doc.ZeroWidthLineBreak + "def"
        );
        await DoTest(
            "abcxdef",
            Doc.FromString("abc") + Doc.LineBreakOr("x") + "def"
        );
        await DoTest(
            "abc\ndef",
            Doc.FromString("abc") + Doc.HardLineBreak + "def"
        );
    }

    [Fact]
    public async Task LineIndentedGroupedWhenFits()
    {
        static Task DoTest(string expected, Doc doc)
            => TestDocument(
                expected,
                doc.Grouped().Nested(2)
            );

        await DoTest(
            "abc def",
            "abc\ndef"
        );
        await DoTest(
            "abc def",
            Doc.FromString("abc") + Doc.LineBreak + "def"
        );
        await DoTest(
            "abcdef",
            Doc.FromString("abc") + Doc.ZeroWidthLineBreak + "def"
        );
        await DoTest(
            "abcxdef",
            Doc.FromString("abc") + Doc.LineBreakOr("x") + "def"
        );
        await DoTest(
            "abc\n  def",
            Doc.FromString("abc") + Doc.HardLineBreak + "def"
        );
    }

    [Fact]
    public void LineGroupedWhenDoesntFit()
    {
        static Task DoTest(Doc doc)
            => TestDocument("abc\ndef", doc.Grouped(), 4);

        DoTest("abc\ndef");
        DoTest(
            Doc.FromString("abc") + Doc.LineBreak + "def"
        );
        DoTest(
            Doc.FromString("abc") + Doc.ZeroWidthLineBreak + "def"
        );
        DoTest(
            Doc.FromString("abc") + Doc.LineBreakOr("x") + "def"
        );
        DoTest(
            Doc.FromString("abc") + Doc.HardLineBreak + "def"
        );
    }

    [Fact]
    public async Task LineIndentedGroupedWhenDoesntFit()
    {
        static Task DoTest(Doc doc)
            => TestDocument("abc\n  def", doc.Grouped().Nested(2), 4);

        await DoTest("abc\ndef");
        await DoTest(
            Doc.FromString("abc") + Doc.LineBreak + "def"
        );
        await DoTest(
            Doc.FromString("abc") + Doc.ZeroWidthLineBreak + "def"
        );
        await DoTest(
            Doc.FromString("abc") + Doc.LineBreakOr("x") + "def"
        );
        await DoTest(
            Doc.FromString("abc") + Doc.HardLineBreak + "def"
        );
    }

    [Fact]
    public async Task NestedGroup()
    {
        await TestDocument(
            "abc def gh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            13
        );

        // outer group should get broken before inner group
        await TestDocument(
            "abc def\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            11
        );

        await TestDocument(
            "abc def\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            7
        );

        await TestDocument(
            "abc\ndef\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            5
        );
        await TestDocument(
            "abc\ndef\ngh\nij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            4
        );
    }

    [Fact]
    public async Task LineAndHardLineWhenFits()
    {
        // A group containing a HardLine can never be flattened
        await TestDocument(
            "abc\ndef\nghi\njkl",
            Doc.Concat(
                "abc", Doc.LineBreak,
                "def", Doc.HardLineBreak,
                "ghi", Doc.LineBreak,
                "jkl"
            ).Grouped()
        );
    }

    [Fact]
    public async Task LineAndHardLineWhenPartialFitAndNestedGroup()
    {
        await TestDocument(
            "abc def\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.HardLineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            7
        );

        await TestDocument(
            "abc\ndef\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.HardLineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            5
        );

        await TestDocument(
            "abc def\nghij\nklmn",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.HardLineBreak,
                Doc.Concat("ghij", Doc.LineBreak, "klmn").Grouped()
            ).Grouped(),
            7
        );
    }

    [Fact]
    public async Task TestAligned()
    {
        var alignedText = new[] { "and now", "some aligned", "text" };
        var alignedDoc = alignedText
            .Select(Doc.FromString)
            .Separated(Doc.LineBreak)
            .Aligned();
        var expected = "some opening words "
            + string.Join("\n" + new string(' ', 19), alignedText);

        await TestDocument(
            expected,
            Doc.Concat(
                "some opening words ",
                alignedDoc
            )
        );
    }

    [Fact]
    public async Task TestAlignedAndGrouped_SingleLine()
    {
        var alignedText = new[] { "and now", "some aligned", "text" };
        var alignedDoc = alignedText
            .Select(Doc.FromString)
            .Separated(Doc.LineBreak)
            .Aligned();
        var expected = "some opening words and now some aligned text";
        var doc = Doc.Concat(
            "some opening words",
            Doc.LineBreak,
            alignedDoc
        ).Grouped();

        await TestDocument(expected, doc);
        await TestDocument(expected, doc, 44);
        await TestDocument(expected, doc, LayoutOptions.Default with { PageWidth = null });
    }

    [Fact]
    public async Task TestAlignedAndGrouped_MultiLine_AlignedBlockOnNewLine()
    {
        var alignedText = new[] { "and now", "some aligned", "text" };
        var alignedDoc = alignedText
            .Select(Doc.FromString)
            .Separated(Doc.LineBreak)
            .Aligned();
        // either all line breaks in a group are taken or none of them are
        var expected = "some opening words\nand now\nsome aligned\ntext";
        var doc = Doc.Concat(
            "some opening words",
            Doc.LineBreak,
            alignedDoc
        ).Grouped();

        await TestDocument(expected, doc, 43);
        await TestDocument(expected, doc, 20);
        await TestDocument(expected, doc, 18);
        // first line overflows but not much the layout algo can do about it
        await TestDocument(expected, doc, 17);
    }

    [Fact]
    public async Task TestAlignedAndGrouped_MultiLine_LineBreakHintOnFirstLine()
    {
        var alignedText = new[] { "and now", "some aligned", "text" };
        var alignedDoc = alignedText
            .Select(Doc.FromString)
            .Separated(Doc.LineBreak)
            .Aligned();
        var expectedAligned = "some opening words "
            + string.Join("\n" + new string(' ', 19), alignedText);
        var doc = Doc.Concat(
            "some opening words",
            Doc.LineBreakHint,
            alignedDoc
        ).Grouped();

        await TestDocument(expectedAligned, doc, 43);
        await TestDocument(expectedAligned, doc, 26);
        // no indentation if the LineBreakHint is taken
        await TestDocument("some opening words\nand now\nsome aligned\ntext", doc, 25);
    }

    // https://github.com/benjamin-hodgson/Gutenberg/issues/4
    [Fact]
    public async Task Issue4_DefaultLayoutModeCanCauseOverflows()
    {
        var alignedText = new[]
        {
            "and now",
            "some aligned",
            "text",
            // this line will overflow (75 + 19 = 94),
            // even though it wouldn't overflow if the
            // layout algorithm had taken one of the
            // LineBreakHints in the first line
            "with an unexpectedly long line later on in the block (after the first line)"
        };
        var alignedDoc = alignedText
            .Select(Doc.FromString)
            .Separated(Doc.LineBreak)
            .Aligned();
        var expected = "some opening words "
            + string.Join("\n" + new string(' ', 19), alignedText);

        await TestDocument(
            expected,
            Doc.Concat(
                // put in some optional line breaks - the layout algorithm won't use them
                Doc.Reflow("some opening words"),
                Doc.LineBreakHint,
                alignedDoc
            )
        );
    }
}
