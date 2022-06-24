namespace Gutenberg.Rendering;

/// <summary>
/// A handy base class for <see cref="IDocumentRenderer{T}"/>
/// implementations which manage annotations in a stack.
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the input <see cref="Document{T}"/>
/// </typeparam>
public abstract class StackMachineDocumentRenderer<T> : IDocumentRenderer<T>
{
    /// <summary>The stack of annotations</summary>
    protected Stack<T> Stack { get; } = new Stack<T>();

    /// <summary>Are there any annotations in the stack?</summary>
    protected bool HasAnnotation
        => Stack.Count > 0;

    /// <summary>
    /// Get the topmost annotation from the stack,
    /// or return a default value.
    /// </summary>
    protected T? CurrentAnnotation
        => HasAnnotation ? Stack.Peek() : default;

    /// <inheritdoc cref="IDocumentRenderer{T}.PushAnnotation"/>
    public virtual ValueTask PushAnnotation(T value, CancellationToken cancellationToken = default)
    {
        Stack.Push(value);
        return CompletedOrCancelled(cancellationToken);
    }
    /// <inheritdoc cref="IDocumentRenderer{T}.PopAnnotation"/>
    public virtual ValueTask PopAnnotation(CancellationToken cancellationToken = default)
    {
        Stack.Pop();
        return CompletedOrCancelled(cancellationToken);
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.Text"/>
    public abstract ValueTask Text(ReadOnlyMemory<char> memory, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IDocumentRenderer{T}.NewLine"/>
    public abstract ValueTask NewLine(CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IDocumentRenderer{T}.WhiteSpace"/>
    public abstract ValueTask WhiteSpace(int amount, CancellationToken cancellationToken = default);

    private static ValueTask CompletedOrCancelled(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled(cancellationToken)
            : ValueTask.CompletedTask;
}
