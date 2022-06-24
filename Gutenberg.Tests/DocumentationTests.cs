using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Gutenberg.Tests;

public class DocumentationTests
{
    [Theory]
    [MemberData(nameof(DiscoverDocTests))]
    public async Task TestXmlDocs(DocTest test)
    {
        var (output, error) = await RunDocTest(test);
        Assert.Equal("", error);
        Assert.Equal(GetExpected(test), SplitLines(output));
    }

    public static IEnumerable<object[]> DiscoverDocTests()
    {
        var path = Path.ChangeExtension(typeof(Document<>).Assembly.Location, "xml");
        var xml = XDocument.Parse(File.ReadAllText(path));
        return (
            from mem in xml.Descendants()
            where mem.Name == "member"
            from ex in mem.Descendants()
            where ex.Name == "example"
            let codes = ex
                .Elements()
                .Where(c => c.Name == "code" && c.Attribute("doctest")?.Value == "true")
                .Select((x, i) => (ix: i, code: x.Value))
            from c in codes
            let name = ex.Attribute("name")!.Value
                + (codes.Count() > 1 ? " > " + c.ix : "")
            select new DocTest(name, c.code)
        ).Distinct().Select(x => new[] { x });
    }

    private static Task<(string output, string error)> RunDocTest(DocTest test)
    {
        const string preamble = "using Doc = Gutenberg.Document<object>;";

        var options = ScriptOptions.Default.AddReferences(
            typeof(Console).Assembly,
            typeof(Document<>).Assembly
        ).AddImports("Gutenberg", "System");

        return RedirectConsole(() => CSharpScript.RunAsync(preamble + test.Code, options));
    }

    private static IEnumerable<string> GetExpected(DocTest test)
    {
        var match = new Regex(@"// Output:\s*").Match(test.Code);
        return SplitLines(test.Code[(match.Index + match.Length)..])
            .Select(line => new Regex(@"^\s*(//( |$))?").Replace(line, ""));
    }

    private static async Task<(string output, string error)> RedirectConsole(Func<Task> action)
    {
        using var outBuffer = new StringWriter();
        using var errBuffer = new StringWriter();
        var oldConsoleOut = Console.Out;
        var oldConsoleErr = Console.Error;
        Console.SetOut(outBuffer);
        Console.SetError(errBuffer);
        try
        {
            await action();
        }
        finally
        {
            Console.SetOut(oldConsoleOut);
            Console.SetError(oldConsoleErr);
        }
        return (outBuffer.ToString(), errBuffer.ToString());
    }

    private static string[] SplitLines(string str)
        => str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
}

public record DocTest(string Name, string Code)
{
    public override string ToString() => Name;
}
