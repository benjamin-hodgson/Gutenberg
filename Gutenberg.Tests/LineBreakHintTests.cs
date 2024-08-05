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
    public void NestedChoicesThenLaterOverflow()
    {
        var doc = Doc.LineBreakHintOr(Doc.ZeroWidthLineBreakHint.Nested(1))
            .Append(new string('a', 5));

        TestDocument("\n" + new string('a', 5), doc, 4);
    }

    [Fact]
    public void GroupedLineBreakHintThenOverflow()
    {
        var doc = Doc.LineBreakHint
            .Grouped()
            .Append(new string('a', 5));

        TestDocument("\naaaaa", doc, 4);
    }
}
