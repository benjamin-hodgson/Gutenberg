namespace Gutenberg;

/// <summary>
/// Contains extension methods for <see cref="Document{T}"/>.
/// </summary>
public static partial class DocumentExtensions
{
    /// <summary>
    /// Returns a <see cref="Document{T}"/> representing the items
    /// in <paramref name="documents"/> concatenated together,
    /// with <paramref name="separator"/> in between them.
    /// </summary>
    ///
    /// <typeparam name="T">The type of annotations in the document.</typeparam>
    /// <param name="documents">The documents to concatenate.</param>
    /// <param name="separator">The document to place between each document.</param>
    /// <returns>
    /// A <see cref="Document{T}"/> representing the items
    /// in <paramref name="documents"/> concatenated together,
    /// with <paramref name="separator"/> in between them.
    /// </returns>
    ///
    /// <example name="Separated example">
    /// <code doctest="true">
    /// var doc = new Doc[] { "item1", "item2", "item3" }
    ///     .Separated(",");
    /// Console.WriteLine(doc);
    /// // Output:
    /// // item1,item2,item3
    /// </code>
    /// </example>
    public static Document<T> Separated<T>(
        this IEnumerable<Document<T>> documents,
        Document<T> separator
    )
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(separator);
        return Document<T>.Concat(documents.Intersperse(separator));
    }

    /// <summary>
    /// Returns a <see cref="Document{T}"/> representing the items
    /// in <paramref name="documents"/> concatenated together,
    /// with <paramref name="separator"/> after each one.
    /// </summary>
    /// <remarks>
    /// <c>docs.SeparatedAndTerminated(sep)</c> is equivalent to
    /// <c>documents.Separated(separator).Append(separator)</c>.
    /// </remarks>
    ///
    /// <typeparam name="T">The type of annotations in the document.</typeparam>
    /// <param name="documents">The documents to concatenate.</param>
    /// <param name="separator">The document to place after each document.</param>
    /// <returns>
    /// A <see cref="Document{T}"/> representing the items
    /// in <paramref name="documents"/> concatenated together,
    /// with <paramref name="separator"/> after each one.
    /// </returns>
    ///
    /// <example name="SeparatedAndTerminated example">
    /// <code doctest="true">
    /// var doc = new Doc[] { "item1", "item2", "item3" }
    ///     .SeparatedAndTerminated(",");
    /// Console.WriteLine(doc);
    /// // Output:
    /// // item1,item2,item3,
    /// </code>
    /// </example>
    public static Document<T> SeparatedAndTerminated<T>(
        this IEnumerable<Document<T>> documents,
        Document<T> separator
    )
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(separator);
        return documents.Separated(separator).Append(separator);
    }
}
