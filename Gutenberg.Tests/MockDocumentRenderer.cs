namespace Gutenberg.Tests;

internal class MockDocumentRenderer<T> : IDocumentRenderer<T>
{
    public List<MockRenderInstruction<T>> Instructions { get; } = new List<MockRenderInstruction<T>>();

    public ValueTask Text(ReadOnlyMemory<char> mem, CancellationToken cancellationToken = default)
    {
        Instructions.Add(new MockRenderInstruction<T>.Text(mem.ToString()));
        return ValueTask.CompletedTask;
    }

    public ValueTask NewLine(CancellationToken cancellationToken = default)
    {
        Instructions.Add(new MockRenderInstruction<T>.NewLine());
        return ValueTask.CompletedTask;
    }

    public ValueTask WhiteSpace(int amount, CancellationToken cancellationToken = default)
    {
        Instructions.Add(new MockRenderInstruction<T>.WhiteSpace(amount));
        return ValueTask.CompletedTask;
    }

    public ValueTask PushAnnotation(T value, CancellationToken cancellationToken = default)
    {
        Instructions.Add(new MockRenderInstruction<T>.PushAnnotation(value));
        return ValueTask.CompletedTask;
    }

    public ValueTask PopAnnotation(CancellationToken cancellationToken = default)
    {
        Instructions.Add(new MockRenderInstruction<T>.PopAnnotation());
        return ValueTask.CompletedTask;
    }
}

internal abstract record MockRenderInstruction<T>
{
    public record NewLine() : MockRenderInstruction<T>;
    public record WhiteSpace(int Amount) : MockRenderInstruction<T>;
    public record PopAnnotation : MockRenderInstruction<T>;
    public record PushAnnotation(T Value) : MockRenderInstruction<T>;
    public record Text(string Value) : MockRenderInstruction<T>;
}
