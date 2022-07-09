using Benjamin.Pizza.DocTest;

namespace Gutenberg.Tests;

public class DocumentationTests
{
    private const string _preamble = @"
        using System.Linq;
        using Gutenberg;
        using Gutenberg.Brackets;
        using Gutenberg.Expression;
        using Doc = Gutenberg.Document<object>;
        using Box = Gutenberg.Box<object>;
    ";

    [Theory]
    [DocTestData(typeof(Document<>), Preamble = _preamble)]
    public async Task TestXmlDocs(DocTest test)
    {
        await test.Run();
    }
}
