using Doc = Gutenberg.Document<object>;
using static Gutenberg.Tests.DocumentTestUtil;

namespace Gutenberg.Tests;

public class LayoutModeSimpleTests
{
    [Fact]
    public async Task TestLayoutModeSimple()
    {
        var doc = Doc.Concat("abc", Doc.LineBreak, "def")
            .Nested(2)
            .Grouped();
        await TestDocument(
            "abc\ndef",
            doc,
            LayoutOptions.Default with { LayoutMode = LayoutMode.Simple }
        );
    }
}
