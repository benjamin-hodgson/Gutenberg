namespace Gutenberg;

/// <summary>
/// An interface for objects which can render themselves
/// as a <see cref="Document{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the resulting <see cref="Document{T}"/>.
/// </typeparam>
public interface IPrettyPrintable<T>
{
    /// <summary>
    /// Render the current object as a <see cref="Document{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Document{T}"/> containing a textual representation
    /// of the current object.
    /// </returns>
    public Document<T> PrettyPrint();
}
