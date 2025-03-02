using CsCheck;

using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

public class PropertyTests : IDisposable
{
    private readonly long _previousIter;

    public PropertyTests()
    {
        _previousIter = Check.Iter;
        Check.Iter = 1000;
    }

    [Fact(Skip = "has a bug")]
    public void Empty()
    {
        Equivalent(Doc.FromString(""), Doc.Empty);
    }

    [Fact]
    public void StringRoundtrip()
    {
        Gen.String.AlphaNumeric
            .Sample(s => Doc.FromString(s).ToString() == s);
    }

    [Fact]
    public void AppendEmpty()
    {
        GenDoc.Equivalent(d => (d, Doc.Empty.Append(d)));
        GenDoc.Equivalent(d => (d, d.Append(Doc.Empty)));
    }

    [Fact]
    public void AppendRespectsStringAdd()
    {
        Gen.Select(Gen.String.AlphaNumeric, Gen.String.AlphaNumeric)
            .Equivalent((s1, s2) => (
                Doc.FromString(s1 + s2),
                Doc.FromString(s1) + Doc.FromString(s2)
            ));
    }

    [Fact]
    public void AppendAssoc()
    {
        Gen.Select(GenDoc, GenDoc, GenDoc)
            .Equivalent((doc1, doc2, doc3) => (
                doc1 + doc2 + doc3,
                doc1 + (doc2 + doc3)
            ));
    }

    [Fact]
    public void NestedZero()
    {
        GenDoc.Equivalent(d => (d.Nested(0), d));
    }

    [Fact]
    public void NestedAdditionDistrib()
    {
        Gen.Select(GenDoc, GenSmallInt, GenSmallInt)
            .Equivalent((doc, x, y) => (
                doc.Nested(x + y),
                doc.Nested(x).Nested(y)
            ));
    }

    [Fact]
    public void NestedEmpty()
    {
        GenSmallInt.Equivalent(n => (Doc.Empty.Nested(n), Doc.Empty));
    }

    [Fact]
    public void NestedAppendDistrib()
    {
        Gen.Select(GenDoc, GenDoc, GenSmallInt)
            .Equivalent((doc1, doc2, n) => (
                doc1.Append(doc2).Nested(n),
                doc1.Nested(n).Append(doc2.Nested(n))
            ));
    }

    [Fact]
    public void NestedString()
    {
        Gen.Select(Gen.String.AlphaNumeric, GenSmallInt)
            .Equivalent((s, n) => (
                Doc.FromString(s).Nested(n),
                Doc.FromString(s)
            ));
    }

    [Fact]
    public void NestedChoiceDistrib()
    {
        Gen.Select(GenDoc, GenDoc, GenSmallInt)
            .Equivalent((doc1, doc2, n) => (
                new ChoiceDocument<object>(doc1, doc2).Nested(n),
                new ChoiceDocument<object>(doc1.Nested(n), doc2.Nested(n))
            ));
    }

    [Fact]
    public void AlignedTwice()
    {
        Gen.Select(GenDoc, GenSmallInt, GenSmallInt)
            .Equivalent((doc, x, y) => (
                doc.Aligned(),
                doc.Aligned().Aligned()
            ));
    }

    [Fact]
    public void GroupedTwice()
    {
        Gen.Select(GenDoc, GenSmallInt, GenSmallInt)
            .Equivalent((doc, x, y) => (
                doc.Grouped(),
                doc.Grouped().Grouped()
            ));
    }

    public void Dispose()
    {
        Check.Iter = _previousIter;
    }
}
