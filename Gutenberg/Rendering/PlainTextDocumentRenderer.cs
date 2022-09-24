namespace Gutenberg.Rendering;

/// <summary>
/// An <see cref="IDocumentRenderer{T}"/> which writes to
/// a <see cref="TextWriter"/> and discards all annotations.
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the document.
/// </typeparam>
public class PlainTextDocumentRenderer<T> : IDocumentRenderer<T>
{
    private static readonly ReadOnlyMemory<char> _whitespace
        = new string(' ', 128).AsMemory();

    /// <summary>The output <see cref="TextWriter"/>.</summary>
    protected TextWriter Writer { get; }

    /// <summary>Create a <see cref="PlainTextDocumentRenderer{T}"/>.</summary>
    /// <param name="writer">The output <see cref="TextWriter"/>.</param>
    public PlainTextDocumentRenderer(TextWriter writer)
    {
        Writer = writer;
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.Text"/>
    public virtual async ValueTask Text(ReadOnlyMemory<char> memory, CancellationToken cancellationToken = default)
    {
        await Writer.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.NewLine"/>
    public virtual async ValueTask NewLine(CancellationToken cancellationToken = default)
    {
        await Writer.WriteAsync(
            "\n".AsMemory(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.WhiteSpace"/>
    public virtual async ValueTask WhiteSpace(int amount, CancellationToken cancellationToken = default)
    {
        await Writer.WriteAsync(
            amount < _whitespace.Length
                ? _whitespace[..amount]
                : new string(' ', amount).AsMemory(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.PushAnnotation"/>
    public virtual ValueTask PushAnnotation(T value, CancellationToken cancellationToken = default)
        => CompletedOrCancelled(cancellationToken);

    /// <inheritdoc cref="IDocumentRenderer{T}.PopAnnotation"/>
    public virtual ValueTask PopAnnotation(CancellationToken cancellationToken = default)
        => CompletedOrCancelled(cancellationToken);

    private static ValueTask CompletedOrCancelled(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled(cancellationToken)
            : ValueTask.CompletedTask;
}

/// <summary>
/// An <see cref="IDocumentRenderer{T}"/> which writes to
/// a <see cref="TextWriter"/> and discards all annotations.
/// </summary>
/// <remarks>
/// This is a convenient subclass of
/// <see cref="PlainTextDocumentRenderer{T}"/> which
/// simply sets the type parameter to <see cref="object"/>,
/// without changing any behaviours.
/// </remarks>
public class PlainTextDocumentRenderer : PlainTextDocumentRenderer<object>
{
    /// <summary>Create a <see cref="PlainTextDocumentRenderer"/>.</summary>
    /// <param name="writer">The output <see cref="TextWriter"/>.</param>
    public PlainTextDocumentRenderer(TextWriter writer)
        : base(writer)
    {
    }
}
