namespace Gutenberg.Bracketing;

/// <summary>
/// An <see cref="IBracketer{T}"/> which uses the K&amp;R style.
/// </summary>
/// <remarks>
/// "The K&amp;R style" is the name for the bracketing style
/// which is customary in C, Java, and JavaScript, among others.
/// The opening bracket is placed on the current line,
/// the contained document is indented, and the closing bracket
/// is placed on a new line.
/// </remarks>
/// <example name="KernighanRitchieBracketer example">
/// <code doctest="true">
/// var bracketer = new KernighanRitchieBracketer&lt;object&gt;("{", "}", false);
/// var doc = "header " + bracketer.Bracket(
///     new Doc[] { "line 1", "line 2" }
///         .Separated(Doc.LineBreak)
/// );
/// Console.WriteLine(doc);
/// // Output:
/// // header {
/// //     line 1
/// //     line 2
/// // }
/// </code>
/// </example>
/// <typeparam name="T">
/// The type of annotations in the document.
/// </typeparam>
/// <param name="OpeningBracket">
/// A <see cref="Document{T}"/> representing the opening
/// bracket character.
/// </param>
/// <param name="ClosingBracket">
/// A <see cref="Document{T}"/> representing the closing
/// bracket character.
/// </param>
/// <param name="Group">
/// Whether the resulting <see cref="Document{T}"/> should
/// be grouped — that is, whether the document may be
/// flattened to a single line if it fits.
/// </param>
/// <param name="NestingLevel">
/// The amount of indentation to apply to the bracketed document
/// </param>
public record KernighanRitchieBracketer<T>(
    Document<T> OpeningBracket,
    Document<T> ClosingBracket,
    bool Group = true,
    int? NestingLevel = null
) : IBracketer<T>
{
    /// <inheritdoc cref="IBracketer{T}.Bracket"/>
    public Document<T> Bracket(Document<T> document)
        => (OpeningBracket + Document<T>.ZeroWidthLineBreak + document)
            .NestBy(NestingLevel)
            .Append(Document<T>.ZeroWidthLineBreak + ClosingBracket)
            .GroupIf(Group);
}
