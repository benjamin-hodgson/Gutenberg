using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

public class GroupingTests
{
    [Fact]
    public void LineGroupedWhenFits()
    {
        static void DoTest(string expected, Doc doc)
            => TestDocument(expected, doc.Grouped());

        DoTest(
            "abc def",
            "abc\ndef"
        );
        DoTest(
            "abc def",
            Doc.FromString("abc") + Doc.LineBreak + "def"
        );
        DoTest(
            "abcdef",
            Doc.FromString("abc") + Doc.ZeroWidthLineBreak + "def"
        );
        DoTest(
            "abcxdef",
            Doc.FromString("abc") + Doc.LineBreakOr("x") + "def"
        );
        DoTest(
            "abc\ndef",
            Doc.FromString("abc") + Doc.HardLineBreak + "def"
        );
    }

    [Fact]
    public void LineIndentedGroupedWhenFits()
    {
        static void DoTest(string expected, Doc doc)
            => TestDocument(
                expected,
                doc.Grouped().Nested(2)
            );

        DoTest(
            "abc def",
            "abc\ndef"
        );
        DoTest(
            "abc def",
            Doc.FromString("abc") + Doc.LineBreak + "def"
        );
        DoTest(
            "abcdef",
            Doc.FromString("abc") + Doc.ZeroWidthLineBreak + "def"
        );
        DoTest(
            "abcxdef",
            Doc.FromString("abc") + Doc.LineBreakOr("x") + "def"
        );
        DoTest(
            "abc\n  def",
            Doc.FromString("abc") + Doc.HardLineBreak + "def"
        );
    }

    [Fact]
    public void LineGroupedWhenDoesntFit()
    {
        static void DoTest(Doc doc)
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
    public void LineIndentedGroupedWhenDoesntFit()
    {
        static void DoTest(Doc doc)
            => TestDocument("abc\n  def", doc.Grouped().Nested(2), 4);

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
    public void NestedGroup()
    {
        TestDocument(
            "abc def gh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            13
        );

        // outer group should get broken before inner group
        TestDocument(
            "abc def\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            11
        );

        TestDocument(
            "abc def\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            7
        );

        TestDocument(
            "abc\ndef\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.LineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            5
        );
        TestDocument(
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
    public void LineAndHardLineWhenFits()
    {
        // A group containing a HardLine can never be flattened
        TestDocument(
            "abc\ndef\nghi\njkl",
            Doc.Concat(
                "abc",
                Doc.LineBreak,
                "def",
                Doc.HardLineBreak,
                "ghi",
                Doc.LineBreak,
                "jkl"
            ).Grouped()
        );
    }

    [Fact]
    public void LineAndHardLineWhenPartialFitAndNestedGroup()
    {
        TestDocument(
            "abc def\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.HardLineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            7
        );

        TestDocument(
            "abc\ndef\ngh ij",
            Doc.Concat(
                Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
                Doc.HardLineBreak,
                Doc.Concat("gh", Doc.LineBreak, "ij").Grouped()
            ).Grouped(),
            5
        );

        TestDocument(
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
    public void TestAligned()
    {
        var alignedText = new[] { "and now", "some aligned", "text" };
        var alignedDoc = alignedText
            .Select(Doc.FromString)
            .Separated(Doc.LineBreak)
            .Aligned();
        var expected = "some opening words "
            + string.Join("\n" + new string(' ', 19), alignedText);

        TestDocument(
            expected,
            Doc.Concat(
                "some opening words ",
                alignedDoc
            )
        );
    }

    [Fact]
    public void TestAlignedAndGrouped_SingleLine()
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

        TestDocument(expected, doc);
        TestDocument(expected, doc, 44);
        TestDocument(expected, doc, LayoutOptions.Default with { PageWidth = null });
    }

    [Fact]
    public void TestAlignedAndGrouped_MultiLine_AlignedBlockOnNewLine()
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

        TestDocument(expected, doc, 43);
        TestDocument(expected, doc, 20);
        TestDocument(expected, doc, 18);

        // first line overflows but not much the layout algo can do about it
        TestDocument(expected, doc, 17);
    }

    [Fact]
    public void TestAlignedAndGrouped_MultiLine_LineBreakHintOnFirstLine()
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

        TestDocument(expectedAligned, doc, 43);
        TestDocument(expectedAligned, doc, 26);

        // no indentation if the LineBreakHint is taken
        TestDocument("some opening words\nand now\nsome aligned\ntext", doc, 25);
    }

    // https://github.com/benjamin-hodgson/Gutenberg/issues/4
    [Fact]
    public void Issue4_DefaultLayoutModeCanCauseOverflows()
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

        // put in some optional line breaks - the layout algorithm won't use them
        TestDocument(
            expected,
            Doc.Concat(
                Doc.Reflow("some opening words"),
                Doc.LineBreakHint,
                alignedDoc
            )
        );
    }
}
