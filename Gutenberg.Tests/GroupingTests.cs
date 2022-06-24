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
}
