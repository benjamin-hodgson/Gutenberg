namespace Gutenberg;

/// <summary>
/// An interface for objects which know how to render
/// a <see cref="Document{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the document.
/// </typeparam>
public interface IDocumentRenderer<in T>
{
    /// <summary>
    /// Write a line break into the output stream.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when the
    /// line break has been written to the output stream.
    /// </returns>
    ValueTask NewLine(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Write the given <paramref name="amount"/> of
    /// blank space into the output stream.
    /// </summary>
    /// <param name="amount">
    /// The amount of white space to write.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when the
    /// white space has been written to the output stream.
    /// </returns>
    ValueTask WhiteSpace(
        int amount,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Write some text into the output stream.
    /// </summary>
    /// <param name="memory">
    /// The text to write to the output stream.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when the
    /// text has been written to the output stream.
    /// </returns>
    ValueTask Text(
        ReadOnlyMemory<char> memory,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Accept an annotation.
    /// </summary>
    /// <param name="value">
    /// The annotation to push onto the stack.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when the
    /// value has been pushed onto the stack.
    /// </returns>
    ValueTask PushAnnotation(
        T value,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Discard the value of a previous call to <see cref="PushAnnotation"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when the
    /// value has been popped from the stack.
    /// </returns>
    ValueTask PopAnnotation(CancellationToken cancellationToken = default);
}
