using Gutenberg.Rendering;

namespace Gutenberg.Tests;

internal class DocumentTestUtil
{
    public static Task TestDocument<T>(string expected, Document<T> doc)
        => TestDocument(expected, doc, LayoutOptions.Default);

    public static Task TestDocument<T>(string expected, Document<T> doc, int pageWidth)
        => TestDocument(expected, doc, LayoutOptions.Default with { PageWidth = new(pageWidth) });

    public static Task TestDocument<T>(string expected, Document<T> doc, LayoutOptions options)
    {
        Assert.Equal(expected, doc.ToString(options));
        return Task.CompletedTask;
    }

    public static Task TestWithMockRenderer<T>(
        Document<T> doc,
        params MockRenderInstruction<T>[] expectedInstructions
    ) => TestWithMockRenderer(doc, x => x, expectedInstructions);

    public static async Task TestWithMockRenderer<T, U>(
        Document<T> doc,
        Func<T, U> mapAnnotations,
        params MockRenderInstruction<U>[] expectedInstructions
    )
    {
        {
            var renderer = new MockDocumentRenderer<U>();
            await doc.Render(renderer.MapAnnotations(mapAnnotations));
            Assert.Equal(expectedInstructions, renderer.Instructions);
        }
    }
}
