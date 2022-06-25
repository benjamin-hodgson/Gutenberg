using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using Xunit.Sdk;

namespace Gutenberg.Tests;

public sealed class DocTestDataAttribute : DataAttribute
{
    public Type TypeInAssemblyToDoctest { get; }
    public string? Preamble { get; set; }

    public DocTestDataAttribute(Type typeInAssemblyToDoctest)
    {
        TypeInAssemblyToDoctest = typeInAssemblyToDoctest;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var assembly = TypeInAssemblyToDoctest.Assembly;
        var path = Path.ChangeExtension(assembly.Location, "xml");
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
            select new DocTest(assembly, name, c.code, Preamble)
        ).Distinct().Select(x => new[] { x });
    }
}

public class DocTest
{
    private readonly Assembly _assembly;
    private readonly string _name;
    private readonly string _code;
    private readonly string? _preamble;

    public DocTest(Assembly assembly, string name, string code, string? preamble)
    {
        _assembly = assembly;
        _name = name;
        _code = code;
        _preamble = preamble;
    }

    public override string ToString() => _name;

    public async Task Run()
    {
        var (output, error) = await RunDocTest();
        Assert.Equal("", error);
        Assert.Equal(GetExpected(), SplitLines(output));
    }

    private Task<(string output, string error)> RunDocTest()
    {
        var options = ScriptOptions.Default.AddReferences(_assembly).AddImports("System");

        return RedirectConsole(() => CSharpScript.RunAsync(_preamble + _code, options));
    }

    private IEnumerable<string> GetExpected()
    {
        var match = new Regex(@"// Output:\s*").Match(_code);
        return SplitLines(_code[(match.Index + match.Length)..])
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


public class DocumentationTests
{
    [Theory]
    [DocTestData(typeof(Document<>), Preamble = "using Gutenberg; using Doc = Gutenberg.Document<object>;")]
    public async Task TestXmlDocs(DocTest test)
    {
        await test.Run();
    }
}
