using System.Globalization;
using System.Text;

namespace Gutenberg.Tests;

internal class FakeDocumentRenderer<T> : IDocumentRenderer<T>
{
    private readonly StringBuilder _sb = new();

    public override string ToString() => _sb.ToString();

    public virtual ValueTask Text(ReadOnlyMemory<char> mem, CancellationToken cancellationToken = default)
    {
        _sb.Append(mem);
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask NewLine(CancellationToken cancellationToken = default)
    {
        _sb.AppendLine();
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask WhiteSpace(int amount, CancellationToken cancellationToken = default)
    {
        _sb.Append(new string(' ', amount));
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask PushAnnotation(T value, CancellationToken cancellationToken = default)
    {
        _sb.Append(CultureInfo.InvariantCulture, $"PUSH({value})");
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask PopAnnotation(CancellationToken cancellationToken = default)
    {
        _sb.Append("POP");
        return ValueTask.CompletedTask;
    }
}
