using Box = Gutenberg.Box<object>;
using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

public class BoxTests
{
    [Fact]
    public void TestFromString()
    {
        {
            var box = Box.FromString("abc");
            Assert.Equal(3, box.Width);
            Assert.Equal(1, box.Height);
            Assert.Equal("abc\n", box.ToString());
        }
        {
            var box = Box.FromString("abc\ndef");
            Assert.Equal(3, box.Width);
            Assert.Equal(2, box.Height);
            Assert.Equal("abc\ndef\n", box.ToString());
        }
        {
            var box = Box.FromString("abc\ndefghi");
            Assert.Equal(6, box.Width);
            Assert.Equal(2, box.Height);
            Assert.Equal("abc   \ndefghi\n", box.ToString());
        }
    }

    [Fact]
    public void TestAbove()
    {
        var box = Box.FromString("abc").Above("def");
        Assert.Equal(3, box.Width);
        Assert.Equal(2, box.Height);
        Assert.Equal("abc\ndef\n", box.ToString());
    }

    [Theory]
    [InlineData(Alignment.Start, "abcdef\nghi   \n")]
    [InlineData(Alignment.CenterStart, "abcdef\n ghi  \n")]
    [InlineData(Alignment.CenterEnd, "abcdef\n  ghi \n")]
    [InlineData(Alignment.End, "abcdef\n   ghi\n")]
    public void TestAboveWithAlignment(Alignment alignment, string expected)
    {
        var box = Box.FromString("abcdef").Above("ghi", alignment);
        Assert.Equal(6, box.Width);
        Assert.Equal(2, box.Height);
        Assert.Equal(expected, box.ToString());
    }

    [Fact]
    public void TestBelow()
    {
        var box = Box.FromString("abc").Below("def");
        Assert.Equal(3, box.Width);
        Assert.Equal(2, box.Height);
        Assert.Equal("def\nabc\n", box.ToString());
    }

    [Theory]
    [InlineData(Alignment.Start, "ghi   \nabcdef\n")]
    [InlineData(Alignment.CenterStart, " ghi  \nabcdef\n")]
    [InlineData(Alignment.CenterEnd, "  ghi \nabcdef\n")]
    [InlineData(Alignment.End, "   ghi\nabcdef\n")]
    public void TestBelowWithAlignment(Alignment alignment, string expected)
    {
        var box = Box.FromString("abcdef").Below("ghi", alignment);
        Assert.Equal(6, box.Width);
        Assert.Equal(2, box.Height);
        Assert.Equal(expected, box.ToString());
    }

    [Fact]
    public void TestLeftOf()
    {
        var box = Box.FromString(string.Join('\n', "abc".AsEnumerable()))
            .LeftOf(string.Join('\n', "def".AsEnumerable()));
        Assert.Equal(2, box.Width);
        Assert.Equal(3, box.Height);
        Assert.Equal("ad\nbe\ncf\n", box.ToString());
    }

    [Theory]
    [InlineData(Alignment.Start, "ag\nbh\nci\nd \ne \nf \n")]
    [InlineData(Alignment.CenterStart, "a \nbg\nch\ndi\ne \nf \n")]
    [InlineData(Alignment.CenterEnd, "a \nb \ncg\ndh\nei\nf \n")]
    [InlineData(Alignment.End, "a \nb \nc \ndg\neh\nfi\n")]
    public void TestLeftOfWithAlignment(Alignment alignment, string expected)
    {
        var box = Box.FromString(string.Join('\n', "abcdef".AsEnumerable()))
            .LeftOf(string.Join('\n', "ghi".AsEnumerable()), alignment);
        Assert.Equal(2, box.Width);
        Assert.Equal(6, box.Height);
        Assert.Equal(expected, box.ToString());
    }

    [Fact]
    public void TestRightOf()
    {
        var box = Box.FromString(string.Join('\n', "abc".AsEnumerable()))
            .RightOf(string.Join('\n', "def".AsEnumerable()));
        Assert.Equal(2, box.Width);
        Assert.Equal(3, box.Height);
        Assert.Equal("da\neb\nfc\n", box.ToString());
    }

    [Theory]
    [InlineData(Alignment.Start, "ga\nhb\nic\n d\n e\n f\n")]
    [InlineData(Alignment.CenterStart, " a\ngb\nhc\nid\n e\n f\n")]
    [InlineData(Alignment.CenterEnd, " a\n b\ngc\nhd\nie\n f\n")]
    [InlineData(Alignment.End, " a\n b\n c\ngd\nhe\nif\n")]
    public void TestRightOfWithAlignment(Alignment alignment, string expected)
    {
        var box = Box.FromString(string.Join('\n', "abcdef".AsEnumerable()))
            .RightOf(string.Join('\n', "ghi".AsEnumerable()), alignment);
        Assert.Equal(2, box.Width);
        Assert.Equal(6, box.Height);
        Assert.Equal(expected, box.ToString());
    }

    [Fact]
    public void TestInFrontOf()
    {
        var mask = Box.Transparent(1, 1)
            .LeftOf("*").RightOf("*")
            .Above("***").Below("***");
        var box = mask
            .InFrontOf("abcde\nfghij\nklmno", Alignment.CenterStart);
        Assert.Equal(5, box.Width);
        Assert.Equal(3, box.Height);
        Assert.Equal("a***e\nf*h*j\nk***o\n", box.ToString());
    }

    [Fact]
    public void TestBehind()
    {
        var mask = Box.Transparent(1, 1)
            .LeftOf("*").RightOf("*")
            .Above("***").Below("***");
        var box = Box.FromString("abcde\nfghij\nklmno")
            .Behind(mask, Alignment.CenterStart);
        Assert.Equal(5, box.Width);
        Assert.Equal(3, box.Height);
        Assert.Equal("a***e\nf*h*j\nk***o\n", box.ToString());
    }

    [Fact]
    public void TestBoxInDocumentWhenFits()
    {
        var box = Box.FromString("abc\ndef\nghi");
        var doc = Doc.FromString("first line ").Append(box);
        Assert.Equal(
            "first line abc\n           def\n           ghi",
            doc.ToString().Trim()  // todo: don't render trailing whitespace
        );
    }

    [Fact]
    public void TestBoxInDocumentWithLineBreak()
    {
        var box = Box.FromString("abc\ndef\nghi");
        var doc = Doc.Concat("first line ", Doc.LineBreak, box);
        Assert.Equal(
            "first line \nabc\ndef\nghi",
            doc.ToString().Trim()  // todo: don't render trailing whitespace
        );
    }

    [Fact]
    public void TestBoxInDocumentWithLineBreakAndNesting()
    {
        var box = Box.FromString("abc\ndef\nghi");
        var doc = Doc.Concat("first line ", Doc.LineBreak, box).Nested(2);
        Assert.Equal(
            "first line \n  abc\n  def\n  ghi",
            doc.ToString().Trim()  // todo: don't render trailing whitespace
        );
    }

    [Fact]
    public void TestBoxInDocumentWithLineBreakHintWhenFits()
    {
        var box = Box.FromString("abc\ndef\nghi");
        var doc = Doc.Concat("first line ", Doc.ZeroWidthLineBreakHint, box);
        Assert.Equal(
            "first line abc\n           def\n           ghi",
            doc.ToString().Trim()  // todo: don't render trailing whitespace
        );
    }

    [Fact]
    public void TestBoxInDocumentWithLineBreakHintWhenDoesntFit()
    {
        var box = Box.FromString("abc\ndef\nghi");
        var doc = Doc.Concat("first line ", Doc.ZeroWidthLineBreakHint, box);
        Assert.Equal(
            "first line \nabc\ndef\nghi",
            doc.ToString(12).Trim()  // todo: don't render trailing whitespace
        );
    }

    [Fact]
    public void TestBoxInDocumentWhenOverflow()
    {
        var box = Box.FromString("abc\ndef\nghi");
        var doc = Doc.Concat("first line ", box);
        Assert.Equal(
            "first line abc\n           def\n           ghi",
            doc.ToString(12).Trim()  // todo: don't render trailing whitespace
        );
    }
}
