namespace Gutenberg.Rendering;

/// <summary>
/// Extension methods for <see cref="IDocumentRenderer{T}"/>
/// </summary>
public static class DocumentRendererExtensions
{
    /// <summary>
    /// Creates an <see cref="IDocumentRenderer{T}"/> which wraps
    /// the specified <paramref name="renderer"/> by applying a
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
    /// <param name="renderer">The wrapped document renderer</param>
    /// <param name="selector">The function to apply to the annotations</param>
    /// <returns>
    /// An <see cref="IDocumentRenderer{T}"/> which wraps an
    /// <paramref name="renderer"/> by applying a
    /// <see cref="Func{T, U}"/> to all of the annotations in
    /// the input <see cref="Document{T}"/>.
    /// </returns>
    public static IDocumentRenderer<T> MapAnnotations<T, U>(
        this IDocumentRenderer<U> renderer,
        Func<T, U> selector
    )
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(selector);
        return new MapAnnotationsDocumentRenderer<T, U>(
            x => ValueTask.FromResult(selector(x)),
            renderer
        );
    }

    /// <summary>
    /// Creates an <see cref="IDocumentRenderer{T}"/> which wraps
    /// the specified <paramref name="renderer"/> by applying an asynchronous
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
    /// <param name="renderer">The wrapped document renderer</param>
    /// <param name="selector">The function to apply to the annotations</param>
    /// <returns>
    /// An <see cref="IDocumentRenderer{T}"/> which wraps an
    /// <paramref name="renderer"/> by applying a
    /// <see cref="Func{T, U}"/> to all of the annotations in
    /// the input <see cref="Document{T}"/>.
    /// </returns>
    public static IDocumentRenderer<T> MapAnnotations<T, U>(
        this IDocumentRenderer<U> renderer,
        Func<T, ValueTask<U>> selector
    )
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(selector);
        return new MapAnnotationsDocumentRenderer<T, U>(selector, renderer);
    }
}
