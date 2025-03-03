using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

public class LineBreakHintTests
{
    [Fact]
    public void TestLineBreakHint()
    {
        TestDocument(
            "abc def",
            Doc.Concat("abc", Doc.LineBreakHint, "def")
        );
        TestDocument(
            "abcdef",
            Doc.Concat("abc", Doc.ZeroWidthLineBreakHint, "def")
        );
        TestDocument(
            "abc|def",
            Doc.Concat("abc", Doc.LineBreakHintOr("|"), "def")
        );

        TestDocument(
            "abc\ndef",
            Doc.Concat("abc", Doc.LineBreakHint, "def"),
            5
        );
        TestDocument(
            "abc\ndef",
            Doc.Concat("abc", Doc.ZeroWidthLineBreakHint, "def"),
            5
        );
        TestDocument(
            "abc\ndef",
            Doc.Concat("abc", Doc.LineBreakHintOr("|"), "def"),
            5
        );
    }

    [Fact]
    public void TestLineBreakHintInGroup()
    {
        var doc = Doc.Concat(
            "abc",
            Doc.LineBreakHint,
            "def",
            Doc.LineBreak,
            "ghi"
        ).Grouped();

        // Attempting to write the LineBreakHint
        // as a linebreak should cause the entire
        // group to be unflattened
        TestDocument("abc\ndef\nghi", doc, 5);
        TestDocument("abc def\nghi", doc, 7);
        TestDocument("abc def ghi", doc, 11);
    }

    [Fact]
    public void TestReflow()
    {
        var doc = Doc.Reflow("hello here are some words");
        TestDocument("hello here are some words", doc);
        TestDocument("hello here are some words", doc, 25);
        TestDocument("hello here are some\nwords", doc, 24);
        TestDocument("hello here are some\nwords", doc, 19);
        TestDocument("hello here are\nsome words", doc, 18);
        TestDocument("hello here\nare some\nwords", doc, 10);
        TestDocument("hello\nhere are\nsome\nwords", doc, 9);
        TestDocument("hello\nhere\nare\nsome\nwords", doc, 7);
        TestDocument("hello\nhere\nare\nsome\nwords", doc, 1);  // overflow
    }

    [Fact]
    public void LineBreakInChoiceAfterForcedOverflow()
    {
        // Should take right branch of choice, even though
        // left branch immediately writes the line break
        TestDocument(
            "aaaaaaaaaaa\n",
            Doc.FromString(new string('a', 11))
                .Append(Doc.LineBreakHintOr(Doc.HardLineBreak.Append("z"))),
            10
        );
    }

    [Fact]
    public void ZeroWidthLineBreakHintAfterForcedOverflow()
    {
        TestDocument(
            "aaaaaaaaaaa\n",
            Doc.FromString(new string('a', 11)).Append(Doc.ZeroWidthLineBreakHint),
            10
        );
    }

    [Fact]
    public void NestedChoices()
    {
        var doc = Doc.LineBreakHintOr(Doc.ZeroWidthLineBreakHint.Nested());

        TestDocument("", doc);
    }

    [Fact]
    public void NestedChoicesThenLaterOverflow()
    {
        var doc = Doc.LineBreakHintOr(Doc.ZeroWidthLineBreakHint.Nested(1))
            .Append(new string('a', 5));

        TestDocument("\n " + new string('a', 5), doc, 4);
    }

    [Fact]
    public void GroupedLineBreakHintThenOverflow()
    {
        var doc = Doc.LineBreakHint
            .Grouped()
            .Append(new string('a', 5));

        TestDocument("\naaaaa", doc, 4);
    }

    [Fact]
    public void TwoLineBreakHintsThenOverflow()
    {
        // 1. Attempt to write "abccccc", realise it'll overflow
        // 2. Backtrack to the "b" and take the line break
        // 3. Attempt to write "a\n", reach EOL and succeed
        // 4. Write " ccccc"; allow overflow to occur since there are no more options for laying out this line
        var d1 = Doc.LineBreakHintOr("a");
        var d2 = Doc.LineBreakHintOr("b").Nested(1);
        var d3 = Doc.FromString(new string('c', 5));

        TestDocument("a\n ccccc", d1 + d2 + d3, 4);
    }
}
