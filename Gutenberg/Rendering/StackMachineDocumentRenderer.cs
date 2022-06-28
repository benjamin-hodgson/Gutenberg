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
    public virtual async ValueTask PushAnnotation(T value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await OnBeforePushAnnotation(value, cancellationToken).ConfigureAwait(false);
        Stack.Push(value);
        await OnPushAnnotation(value, cancellationToken).ConfigureAwait(false);
    }
    /// <inheritdoc cref="IDocumentRenderer{T}.PopAnnotation"/>
    public virtual async ValueTask PopAnnotation(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await OnBeforePopAnnotation(Stack.Peek(), cancellationToken).ConfigureAwait(false);
        var val = Stack.Pop();
        await OnPopAnnotation(val, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.Text"/>
    public abstract ValueTask Text(ReadOnlyMemory<char> memory, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IDocumentRenderer{T}.NewLine"/>
    public abstract ValueTask NewLine(CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IDocumentRenderer{T}.WhiteSpace"/>
    public abstract ValueTask WhiteSpace(int amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when <paramref name="value"/> is about to be
    /// pushed onto the <see cref="Stack"/>.
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will be awaited before
    /// pushing the <paramref name="value"/> on to the <see cref="Stack"/>
    /// </returns>
    protected virtual ValueTask OnBeforePushAnnotation(T value, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <summary>
    /// Called immediately after <paramref name="value"/> has been
    /// pushed onto the <see cref="Stack"/>.
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will be awaited before
    /// returning from <see cref="PushAnnotation"/>
    /// </returns>
    protected virtual ValueTask OnPushAnnotation(T value, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <summary>
    /// Called when <paramref name="value"/> is about to be
    /// popped off the <see cref="Stack"/>.
    /// </summary>
    /// <remarks>
    /// If the stack is mutated (by an override of this method) before the returned
    /// <see cref="ValueTask"/> completes, the value which is on top of the stack
    /// after the <see cref="ValueTask"/> resolves will be popped (and
    /// passed to <see cref="OnPopAnnotation"/>) instead.
    /// </remarks>
    /// <param name="value">The value</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will be awaited before
    /// popping the <paramref name="value"/> off the <see cref="Stack"/>
    /// </returns>
    protected virtual ValueTask OnBeforePopAnnotation(T value, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <summary>
    /// Called immediately after <paramref name="value"/> has been
    /// popped off the <see cref="Stack"/>.
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will be awaited before
    /// returning from <see cref="PopAnnotation"/>
    /// </returns>
    protected virtual ValueTask OnPopAnnotation(T value, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
