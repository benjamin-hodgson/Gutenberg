using Benjamin.Pizza.DocTest;

namespace Gutenberg.Tests;

public class DocumentationTests
{
    private const string _preamble = @$"
        using System.Linq;
        using {nameof(Gutenberg)};
        using {nameof(Gutenberg)}.{nameof(Brackets)};
        using {nameof(Gutenberg)}.{nameof(Expression)};
        using Doc = {nameof(Gutenberg)}.{nameof(Document<object>)}<object>;
        using Box = {nameof(Gutenberg)}.{nameof(Box<object>)}<object>;
    ";

    [Theory]
    [DocTestData(typeof(Document<>), Preamble = _preamble)]
    public async Task TestXmlDocs(DocTest test)
    {
        ArgumentNullException.ThrowIfNull(test);
        await test.Run();
    }
}
