namespace Gutenberg.Rendering;

/// <summary>
/// An <see cref="IDocumentRenderer{T}"/> which writes to
/// a <see cref="TextWriter"/> and discards all annotations.
/// </summary>
public class PlainTextDocumentRenderer : IDocumentRenderer<object>
{
    /// <summary>The output <see cref="TextWriter"/></summary>
    protected TextWriter Writer { get; }

    /// <summary>Create a <see cref="PlainTextDocumentRenderer"/></summary>
    /// <param name="writer">The output <see cref="TextWriter"/></param>
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
            new string(' ', amount).AsMemory(),  // fixme: pool this
            cancellationToken
        ).ConfigureAwait(false);
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.PushAnnotation"/>
    public virtual ValueTask PushAnnotation(object value, CancellationToken cancellationToken = default)
        => CompletedOrCancelled(cancellationToken);

    /// <inheritdoc cref="IDocumentRenderer{T}.PopAnnotation"/>
    public virtual ValueTask PopAnnotation(CancellationToken cancellationToken = default)
        => CompletedOrCancelled(cancellationToken);

    private static ValueTask CompletedOrCancelled(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled(cancellationToken)
            : ValueTask.CompletedTask;
}
