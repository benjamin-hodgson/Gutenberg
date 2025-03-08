using System.Diagnostics.CodeAnalysis;

using Benjamin.Pizza.DocTest;

namespace Gutenberg.Tests;

#pragma warning disable SA1010  // Opening square brackets should not be preceded by a space
[DocTest(
    typeof(Document<>),
    Usings = [
        "System.Linq",
        $"{nameof(Gutenberg)}",
        $"{nameof(Gutenberg)}.{nameof(Brackets)}",
        $"{nameof(Gutenberg)}.{nameof(Expression)}",
        $"Doc = {nameof(Gutenberg)}.{nameof(Document<object>)}<object>",
        $"Box = {nameof(Gutenberg)}.{nameof(Box<object>)}<object>",
    ]
)]
#pragma warning restore SA1010  // Opening square brackets should not be preceded by a space
[SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Doctest")]
public partial class DocumentationTests
{
}
