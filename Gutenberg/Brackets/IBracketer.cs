namespace Gutenberg.Brackets;

/// <summary>
/// An interface for objects which know how to surround
/// a <see cref="Document{T}"/> with brackets.
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the document.
/// </typeparam>
public interface IBracketer<T>
{
    /// <summary>
    /// Surrounds the <paramref name="document"/> with brackets.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The document surrounded with brackets.</returns>
    Document<T> Bracket(Document<T> document);
}
