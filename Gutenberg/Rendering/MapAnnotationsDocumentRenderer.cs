namespace Gutenberg.Rendering;

/// <summary>
/// An <see cref="IDocumentRenderer{T}"/> which wraps an
/// <see cref="IDocumentRenderer{U}"/> by applying an asynchronous
/// <see cref="Func{T, U}"/> to all of the annotations in
/// the input <see cref="Document{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the input <see cref="Document{T}"/>.
/// </typeparam>
/// <typeparam name="U">
/// The type of annotations accepted by the wrapped
/// <see cref="IDocumentRenderer{U}"/>.
/// </typeparam>
public class MapAnnotationsDocumentRenderer<T, U> : IDocumentRenderer<T>
{
    private readonly Func<T, ValueTask<U>> _selector;
    private readonly IDocumentRenderer<U> _wrapped;

    /// <summary>Creates a <see cref="MapAnnotationsDocumentRenderer{T, U}"/>.</summary>
    /// <param name="selector">The function to apply to the annotations.</param>
    /// <param name="wrapped">The wrapped document renderer.</param>
    public MapAnnotationsDocumentRenderer(Func<T, ValueTask<U>> selector, IDocumentRenderer<U> wrapped)
    {
        _selector = selector;
        _wrapped = wrapped;
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.NewLine"/>
    public ValueTask NewLine(CancellationToken cancellationToken = default)
        => _wrapped.NewLine(cancellationToken);

    /// <inheritdoc cref="IDocumentRenderer{T}.WhiteSpace"/>
    public ValueTask WhiteSpace(int amount, CancellationToken cancellationToken = default)
        => _wrapped.WhiteSpace(amount, cancellationToken);

    /// <inheritdoc cref="IDocumentRenderer{T}.Text"/>
    public ValueTask Text(ReadOnlyMemory<char> memory, CancellationToken cancellationToken = default)
        => _wrapped.Text(memory, cancellationToken);

    /// <inheritdoc cref="IDocumentRenderer{T}.PushAnnotation"/>
    public async ValueTask PushAnnotation(T value, CancellationToken cancellationToken = default)
    {
        var newVal = await _selector(value).ConfigureAwait(false);
        await _wrapped.PushAnnotation(newVal, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.PopAnnotation"/>
    public ValueTask PopAnnotation(CancellationToken cancellationToken = default)
        => _wrapped.PopAnnotation(cancellationToken);
}
