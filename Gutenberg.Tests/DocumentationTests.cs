using Benjamin.Pizza.DocTest;

namespace Gutenberg.Tests;

public class DocumentationTests
{
    [Theory]
    [DocTestData(typeof(Document<>), Preamble = "using Gutenberg; using Doc = Gutenberg.Document<object>;")]
    public async Task TestXmlDocs(DocTest test)
    {
        await test.Run();
    }
}
