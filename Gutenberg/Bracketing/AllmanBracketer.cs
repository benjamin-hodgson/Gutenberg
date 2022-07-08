using System.Diagnostics.CodeAnalysis;

namespace Gutenberg.Bracketing;

/// <summary>
/// An <see cref="IBracketer{T}"/> which uses the Allman style.
/// </summary>
/// <remarks>
/// "The Allman style" is the name for the bracketing style
/// which is customary in C#. 
/// The opening bracket is placed on a new line,
/// the contained document is indented, and the closing bracket
/// is placed on a new line.
/// </remarks>
/// <example name="AllmanBracketer example">
/// <code doctest="true">
/// var bracketer = new AllmanBracketer&lt;object&gt;("{", "}", false);
/// var doc = "header" + bracketer.Bracket(
///     new Doc[] { "line 1", "line 2" }
///         .Separated(Doc.LineBreak)
/// );
/// Console.WriteLine(doc);
/// // Output:
/// // header
/// // {
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
public record AllmanBracketer<T>(
    Document<T> OpeningBracket,
    Document<T> ClosingBracket,
    bool Group = true,
    int? NestingLevel = null
) : IBracketer<T>
{
    /// <inheritdoc cref="IBracketer{T}.Bracket"/>
    public Document<T> Bracket(Document<T> document)
        => Document<T>.Concat(
            Document<T>.ZeroWidthLineBreak + OpeningBracket,
            (Document<T>.ZeroWidthLineBreak + document).NestBy(NestingLevel),
            Document<T>.ZeroWidthLineBreak + ClosingBracket
        ).GroupIf(Group);

    /// <summary>
    /// An <see cref="IBracketer{T}"/> which uses the Allman
    /// style with round parentheses (<c>()</c>), and allows
    /// the bracketed document to be grouped.
    /// </summary>
    [SuppressMessage("design", "CA1000")]  // "Do not declare static members on generic types"
    public static AllmanBracketer<T> Default { get; }
        = new("(", ")");
}
