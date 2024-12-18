using CsCheck;

using Gutenberg.Rendering;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

internal static class DocumentTestUtil
{
    public static void TestDocument<T>(string expected, Document<T> doc)
        => TestDocument(expected, doc, LayoutOptions.Default);

    public static void TestDocument<T>(string expected, Document<T> doc, int pageWidth)
        => TestDocument(expected, doc, LayoutOptions.Default with { PageWidth = new(pageWidth) });

    public static void TestDocument<T>(string expected, Document<T> doc, LayoutOptions options)
    {
        Assert.Equal(expected, doc.ToString(options));
    }

    public static Task<string> ObserveAnnotations<T>(Document<T> doc)
        => ObserveAnnotations(doc, x => x);

    public static async Task<string> ObserveAnnotations<T, U>(
        Document<T> doc,
        Func<T, U> mapAnnotations
    )
    {
        var renderer = new FakeDocumentRenderer<U>();
        await doc.Render(renderer.MapAnnotations(mapAnnotations), TestContext.Current.CancellationToken);
        return renderer.ToString();
    }

    public static Gen<int> GenSmallInt { get; } = Gen.Int[0, 1024];

    // don't run property tests in smart layout mode
    private static readonly Gen<LayoutOptions> _genLayoutOptions
        = Gen.Select(
            Gen.Int[10, 150],
            Gen.Bool,
            (w, s) => new LayoutOptions(new PageWidthOptions(w), LayoutMode.Default, 4, s));

    private static readonly Gen<Doc> _genSimpleDoc = Gen.OneOf(
        Gen.Const(Doc.Empty),
        Gen.Const(Doc.HardLineBreak),
        GenSmallInt.Select(n => new WhiteSpaceDocument<object>(n)),
        Gen.String.Select(Doc.FromString)
    );

    public static Gen<Func<Doc, Doc>> GenNested { get; }
        = Gen.Recursive<Func<Doc, Doc>>(
            (depth, rec) =>
            {
                var leaf = Gen.Const<Func<Doc, Doc>>(z => z);
                if (depth == 5)
                {
                    return leaf;
                }

                return Gen.OneOf(
                    leaf,
                    _genSimpleDoc.Select<Doc, Func<Doc, Doc>>(d => _ => d),
                    rec.Select<Func<Doc, Doc>, Func<Doc, Doc>>(d => z => d(z).Grouped()),
                    rec.Select<Func<Doc, Doc>, Func<Doc, Doc>>(d => z => d(z).Aligned()),
                    Gen.Select(rec, GenSmallInt).Select<Func<Doc, Doc>, int, Func<Doc, Doc>>((d, n) => z => d(z).Nested(n)),
                    Gen.Select(rec, rec).Select<Func<Doc, Doc>, Func<Doc, Doc>, Func<Doc, Doc>>((x, y) => z => new AlternativeDocument<object>(x(z), y(z))),
                    Gen.Select(rec, rec).Select<Func<Doc, Doc>, Func<Doc, Doc>, Func<Doc, Doc>>((x, y) => z => new ChoiceDocument<object>(x(z), y(z))),
                    Gen.Select(rec, rec).Select<Func<Doc, Doc>, Func<Doc, Doc>, Func<Doc, Doc>>((l, r) => z => l(z).Append(r(z)))
                );
            }
        );

    public static Gen<Doc> GenDoc { get; }
        = _genSimpleDoc.Select(GenNested, (z, f) => f(z));

    public static void Equivalent<T>(this Gen<T> gen, Func<T, (Doc doc1, Doc doc2)> doc)
    {
        var g =
            from t in gen
            let docs = doc(t)
            from f in GenNested
            from options in _genLayoutOptions
            select (options, f(docs.doc1), f(docs.doc2));

        g.Sample(
            (options, doc1, doc2) => Assert.Equal(doc1.ToString(options), doc2.ToString(options)),
            print: t => $"({t.Item1}, {t.Item2.Display()}, {t.Item3.Display()})"
        );
    }

    public static void Equivalent<T1, T2>(this Gen<(T1, T2)> gen, Func<T1, T2, (Doc doc1, Doc doc2)> doc)
        => gen.Equivalent(t => doc(t.Item1, t.Item2));

    public static void Equivalent<T1, T2, T3>(this Gen<(T1, T2, T3)> gen, Func<T1, T2, T3, (Doc doc1, Doc doc2)> doc)
        => gen.Equivalent(t => doc(t.Item1, t.Item2, t.Item3));
}
