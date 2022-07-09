namespace Gutenberg.Brackets;

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
public class AllmanBracketer<T> : IBracketer<T>
{
    private readonly Document<T> _openingBracket;
    private readonly Document<T> _closingBracket;
    private readonly bool _group;
    private readonly int? _nestingLevel;

    /// <summary>
    /// Creates an <see cref="IBracketer{T}"/> which uses the Allman style.
    /// </summary>
    /// <param name="openingBracket">
    /// A <see cref="Document{T}"/> representing the opening
    /// bracket character.
    /// </param>
    /// <param name="closingBracket">
    /// A <see cref="Document{T}"/> representing the closing
    /// bracket character.
    /// </param>
    /// <param name="group">
    /// Whether the resulting <see cref="Document{T}"/> should
    /// be grouped — that is, whether the document may be
    /// flattened to a single line if it fits.
    /// </param>
    /// <param name="nestingLevel">
    /// The amount of indentation to apply to the bracketed document
    /// </param>
    public AllmanBracketer(
        Document<T> openingBracket,
        Document<T> closingBracket,
        bool group = true,
        int? nestingLevel = null
    )
    {
        _openingBracket = openingBracket;
        _closingBracket = closingBracket;
        _group = group;
        _nestingLevel = nestingLevel;
    }

    /// <inheritdoc cref="IBracketer{T}.Bracket"/>
    public Document<T> Bracket(Document<T> document)
        => Document<T>.Concat(
            Document<T>.ZeroWidthLineBreak + _openingBracket,
            (Document<T>.ZeroWidthLineBreak + document).NestBy(_nestingLevel),
            Document<T>.ZeroWidthLineBreak + _closingBracket
        ).GroupIf(_group);
}
