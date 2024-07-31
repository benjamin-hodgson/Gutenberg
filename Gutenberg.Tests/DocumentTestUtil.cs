using Gutenberg.Rendering;

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
        await doc.Render(renderer.MapAnnotations(mapAnnotations));
        return renderer.ToString();
    }
}
