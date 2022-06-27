using Benjamin.Pizza.DocTest;

namespace Gutenberg.Tests;

public class DocumentationTests
{
    [Theory]
    [DocTestData(typeof(Document<>), Preamble = "using System.Linq; using Gutenberg; using Doc = Gutenberg.Document<object>; using Box = Gutenberg.Box<object>;")]
    public async Task TestXmlDocs(DocTest test)
    {
        await test.Run();
    }
}
