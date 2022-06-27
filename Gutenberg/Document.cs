using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Gutenberg.Rendering;

namespace Gutenberg;

/// <summary>
/// <para>
/// Represents an immutable textual document which can be laid out
/// in a variety of ways. Once laid out, the document can be
/// rendered by an <see cref="IDocumentRenderer{T}"/>.
/// </para>
/// 
/// <para>
/// A <see cref="Document{T}"/>'s layout is determined by
/// the available page width, the locations of line breaks
/// within the document, and the locations of groups within
/// the document.
/// </para>
/// 
/// <para>
/// The page has a certain maximum width
/// (determined by <see cref="LayoutOptions.PageWidth"/>),
/// which the layout algorithm tries not to exceed, by
/// inserting line breaks where possible. The possible line
/// breaks are indicated by the presence of <see cref="LineBreak"/>
/// values within the document.
/// </para>
///
/// <para>
/// A document may contain groups, introduced by the <see cref="Grouped"/>
/// method. A group indicates a "flattenable" region within a document.
/// When the layout algorithm encounters a group, it will attempt to
/// flatten the group into a single line. If the result does not fit
/// within the page width, the group is rendered without changes.
/// Groups give the layout algorithm flexibility to choose the "best" way
/// to display a document, by using the available horizontal space
/// efficiently.
/// </para>
///
/// <para>
/// The document may contain annotations - values of type
/// <typeparamref name="T"/> - which can be interpreted by the
/// <see cref="IDocumentRenderer{T}"/>.
/// </para>
///
/// <para>
/// This class is intended to be imported under an alias,
/// since typically the type of annotations won't change within your code:
/// <c>using Doc = Gutenberg.Document&lt;MyAnnotation&gt;;</c>
/// </para>
/// </summary>
/// <remarks>
/// The examples in the documentation for this class are assumed to be preceded by
/// <c>using Doc = Gutenberg.Document&lt;object&gt;;</c>
/// </remarks>
/// <typeparam name="T">
/// The type of annotations in the document.
/// </typeparam>
[SuppressMessage(
    "Design",
    "CA1000",  // "Do not declare static members on generic types"
    Justification = "This type is designed to be imported under an alias"
)]
public abstract class Document<T> : IStackItem<T>
{
    // null for unflattenable
    internal int? FlattenedWidth { get; }

    private protected Document(int? flattenedWidth)
    {
        FlattenedWidth = flattenedWidth;
    }

    /// <summary>
    /// Lay out the <see cref="Document{T}"/>,
    /// using the default <see cref="LayoutOptions"/>,
    /// and write its text to the <paramref name="renderer"/>.
    /// </summary>
    /// <param name="renderer">
    /// The <see cref="IDocumentRenderer{T}"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when all of
    /// the <see cref="Document{T}"/>'s text has been written to
    /// the <paramref name="renderer"/>.
    /// </returns>
    public ValueTask Render(IDocumentRenderer<T> renderer, CancellationToken cancellationToken = default)
        => Render(LayoutOptions.Default, renderer, cancellationToken);

    /// <summary>
    /// Lay out the <see cref="Document{T}"/>,
    /// with the given <paramref name="pageWidth"/>, and
    /// write its text to the <paramref name="renderer"/>.
    /// </summary>
    /// <param name="pageWidth">The page width</param>
    /// <param name="renderer">
    /// The <see cref="IDocumentRenderer{T}"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when all of
    /// the <see cref="Document{T}"/>'s text has been written to
    /// the <paramref name="renderer"/>.
    /// </returns>
    public ValueTask Render(int pageWidth, IDocumentRenderer<T> renderer, CancellationToken cancellationToken = default)
        => Render(
            LayoutOptions.Default with { PageWidth = new(pageWidth) },
            renderer,
            cancellationToken
        );

    /// <summary>
    /// Lay out the <see cref="Document{T}"/> and write its
    /// text to the <paramref name="renderer"/>.
    /// </summary>
    /// <param name="options">Options for rendering the document</param>
    /// <param name="renderer">
    /// The <see cref="IDocumentRenderer{T}"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> which will complete when all of
    /// the <see cref="Document{T}"/>'s text has been written to
    /// the <paramref name="renderer"/>.
    /// </returns>
    public ValueTask Render(LayoutOptions options, IDocumentRenderer<T> renderer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(renderer);

        switch (options.LayoutMode)
        {
            case LayoutMode.Default:
                var engine = new LayoutEngine<T>(options, renderer);
                return engine.Layout(this, cancellationToken);
            case LayoutMode.Simple:
                return RenderSimple(renderer, cancellationToken);
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(options.LayoutMode),
                    options.LayoutMode,
                    $"unknown {nameof(LayoutMode)}"
                );
        }
    }

    /// <summary>
    /// Lay out the <see cref="Document{T}"/>,
    /// using the default <see cref="LayoutOptions"/>,
    /// and render it as a string.
    /// </summary>
    public override string ToString()
        => ToString(LayoutOptions.Default);

    /// <summary>
    /// Lay out the <see cref="Document{T}"/>,
    /// with the given <paramref name="pageWidth"/>,
    /// and render it as a string.
    /// </summary>
    public string ToString(int pageWidth)
        => ToString(LayoutOptions.Default with { PageWidth = new(pageWidth) });

    /// <summary>
    /// Lay out the <see cref="Document{T}"/> and render it as a string.
    /// </summary>
    public string ToString(LayoutOptions options)
    {
        using var writer = new StringWriter();

        var task = Render(options, (IDocumentRenderer<T>)new PlainTextDocumentRenderer(writer));

        // neither of these should ever happen
        if (!task.IsCompleted)
        {
            throw new InvalidOperationException("ToString went async! Please report this as a bug in Gutenberg");
        }
        if (task.IsFaulted)
        {
            throw new InvalidOperationException(
                "ToString threw! Please report this as a bug in Gutenberg.",
                task.AsTask().Exception
            );
        }

        return writer.ToString();
    }

    /// <summary>
    /// Returns a new <see cref="Document{T}"/> representing
    /// the content of this document concatenated with the
    /// content of <paramref name="other"/>.
    /// </summary>
    /// <remarks>
    /// <c>x.Append(y)</c> is equivalent to
    /// <c>Document&lt;T&gt;.Concat(x, y)</c>.
    /// </remarks>
    /// <example name="Append example">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.FromString("abc").Append("def"));
    /// // Output:
    /// // abcdef
    /// </code>
    /// </example>
    /// <param name="other">
    /// The document to append to this document
    /// </param>
    /// <returns>
    /// A new <see cref="Document{T}"/> representing
    /// the content of this document concatenated with the
    /// content of <paramref name="other"/>.
    /// </returns>
    /// <seealso cref="Concat(Document{T}[])"/>
    public Document<T> Append(Document<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (this is EmptyDocument<T>)
        {
            return other;
        }
        if (other is EmptyDocument<T>)
        {
            return this;
        }
        return new AppendDocument<T>(this, other);
    }

    /// <summary>
    /// Returns a new <see cref="Document{T}"/> representing
    /// the content of the current document surrounded by the
    /// contents of <paramref name="before"/> and <paramref name="after"/>.
    /// </summary>
    /// <example name="Between example">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.FromString("abc").Between("(", ")"));
    /// // Output:
    /// // (abc)
    /// </code>
    /// </example>
    /// <param name="before">
    /// The document to place before the current document
    /// </param>
    /// <param name="after">
    /// The document to place after the current document
    /// </param>
    /// <returns>
    /// A new <see cref="Document{T}"/> representing
    /// the content of the current document surrounded by the
    /// contents of <paramref name="before"/> and <paramref name="after"/>.
    /// </returns>
    public Document<T> Between(Document<T> before, Document<T> after)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        return Concat(ImmutableArray.Create(before, this, after));
    }

    /// <summary>
    /// <para>
    /// Signals that the current <see cref="Document{T}"/>
    /// is eligible to be flattened.
    /// </para>
    /// 
    /// <para>
    /// When the layout algorithm encounters a <see cref="Grouped"/>
    /// region of a document, it attempts to flatten that region by
    /// removing any line breaks within it. If the flattened version
    /// of the document does not fit within the page width, the
    /// original document will be rendered without any changes.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The appearance of the flattened version of the document
    /// depends on the variety of line breaks within it:
    /// <see cref="LineBreak"/> flattens to a single space;
    /// <see cref="ZeroWidthLineBreak"/> flattens to nothing.
    /// </remarks>
    /// <example name="Grouped example - fits">
    /// Here is an example of a document containing
    /// <see cref="LineBreak"/>s, which is rendered on a single line
    /// because it has been grouped.
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "abc", Doc.LineBreak,
    ///     "def", Doc.LineBreak,
    ///     "ghi"
    /// ).Grouped();
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc def ghi
    /// </code>
    /// </example>
    /// <example name="Grouped example - does not fit">
    /// Here, the document from the previous example does not
    /// fit on a single line due to the narrow page width.
    /// The original <see cref="LineBreak"/>s are rendered unchanged.
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "abc", Doc.LineBreak,
    ///     "def", Doc.LineBreak,
    ///     "ghi"
    /// ).Grouped();
    /// Console.WriteLine(doc.ToString(3));
    /// // Output:
    /// // abc
    /// // def
    /// // ghi
    /// </code>
    /// The group is flattened atomically - either all of the line
    /// breaks within the group are flattened or the document is rendered
    /// unchanged. In this example, the page is wide enough for the
    /// first two line breaks to be flattened, but not the third.
    /// However, all three line breaks are rendered as line breaks.
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "abc", Doc.LineBreak,
    ///     "def", Doc.LineBreak,
    ///     "ghi"
    /// ).Grouped();
    /// Console.WriteLine(doc.ToString(7));
    /// // Output:
    /// // abc
    /// // def
    /// // ghi
    /// </code>
    /// </example>
    /// <example name="Nested groups">
    /// Groups may be nested within each other arbitrarily.
    /// If a group fits within a line, the entire group will
    /// be flattened, including any groups nested within it.
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
    ///     Doc.LineBreak,
    ///     Doc.Concat("ghi", Doc.LineBreak, "jkl").Grouped()
    /// ).Grouped();
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc def ghi jkl
    /// </code>
    /// If a parent group doesn't fit within the page width, the child groups
    /// are still eligibe to be flattened.
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     Doc.Concat("abc", Doc.LineBreak, "def").Grouped(),
    ///     Doc.LineBreak,
    ///     Doc.Concat("ghi", Doc.LineBreak, "jkl").Grouped()
    /// ).Grouped();
    /// Console.WriteLine(doc.ToString(7));
    /// // Output:
    /// // abc def
    /// // ghi jkl
    /// </code>
    /// </example>
    public Document<T> Grouped()
        => new ChoiceDocument<T>(new FlattenedDocument<T>(this), this);

    /// <summary>
    /// <para>
    /// Increase the nesting level of the current <see cref="Document{T}"/>
    /// by <paramref name="amount"/>.
    /// </para>
    ///
    /// <para>
    /// A document's nesting level indicates the amount of indentation
    /// which should be applied to line breaks within the document.
    /// </para>
    /// </summary>
    /// <example name="Nesting is not immediate">
    /// A nested document is not "immediately" indented. Rather, the
    /// given amount of nesting is applied to any line breaks within
    /// the document. If you nest a document which has some text on
    /// its first line, that text is displayed unaltered. The following
    /// lines in the document will have the nesting applied to them.
    /// <code doctest="true">
    /// var doc = new Doc[]
    /// {
    ///     "first line",
    ///     "second line",
    ///     "third line"
    /// }.Separated(Doc.LineBreak).Nested(4);
    /// Console.WriteLine(doc);
    /// // Output:
    /// // first line
    /// //     second line
    /// //     third line
    /// </code>
    /// This also applies when there is already text on the current line:
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "leading text ",
    ///     new Doc[]
    ///     {
    ///         "first line",
    ///         "second line",
    ///         "third line"
    ///     }.Separated(Doc.LineBreak).Nested(4)
    /// );
    /// Console.WriteLine(doc);
    /// // Output:
    /// // leading text first line
    /// //     second line
    /// //     third line
    /// </code>
    /// </example>
    /// <param name="amount">
    /// The amount by which to increase the document's nesting level
    /// </param>
    /// <returns>
    /// A new <see cref="Document{T}"/> representing the current document
    /// with its nesting level increased by <paramref name="amount"/>.
    /// </returns>
    /// <seealso cref="Nested()"/>
    public Document<T> Nested(int amount)
        => new NestedDocument<T>(amount, this);

    /// <summary>
    /// <para>
    /// Increase the nesting level of the current <see cref="Document{T}"/>
    /// by <see cref="LayoutOptions.DefaultNesting"/>.
    /// </para>
    ///
    /// <para>
    /// A document's nesting level indicates the amount of indentation
    /// which should be applied to line breaks within the document.
    /// </para>
    /// </summary>
    /// <example name="Default indentation level">
    /// <code doctest="true">
    /// var doc = new Doc[]
    /// {
    ///     "first line",
    ///     "second line",
    ///     "third line"
    /// }.Separated(Doc.LineBreak).Nested();
    /// var options = LayoutOptions.Default with { DefaultNesting = 2 };
    /// Console.WriteLine(doc.ToString(options));
    /// // Output:
    /// // first line
    /// //   second line
    /// //   third line
    /// </code>
    /// </example>
    /// <returns>
    /// A new <see cref="Document{T}"/> representing the current document
    /// with its nesting level increased by <see cref="LayoutOptions.DefaultNesting"/>.
    /// </returns>
    /// <seealso cref="Nested(int)"/>
    public Document<T> Nested()
        => new NestedDocument<T>(null, this);

    /// <summary>
    /// Sets the nesting level of the document to the current column.
    /// </summary>
    /// <remarks>
    /// For many purposes, <see cref="Nested(int)"/> is both simpler
    /// and more efficient than <see cref="Aligned"/>.
    /// </remarks>
    /// <example name="Aligned example">
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "leading text ",
    ///     new Doc[]
    ///     {
    ///         "first line",
    ///         "second line",
    ///         "third line"
    ///     }.Separated(Doc.LineBreak).Aligned()
    /// );
    /// Console.WriteLine(doc);
    /// // Output:
    /// // leading text first line
    /// //              second line
    /// //              third line
    /// </code>
    /// </example>
    public Document<T> Aligned()
        => new AlignedDocument<T>(this);

    /// <summary>
    /// Aligns the current document and indents it by
    /// <paramref name="indentation"/>, starting at the current column.
    /// </summary>
    /// <remarks>
    /// <c>doc.Indented(amt)</c> is equivalent to
    /// <c>Doc.FromString(new string(' ', amt)) + doc.Aligned()</c>.
    /// </remarks>
    /// <example name="Indented example">
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "leading text ",
    ///     new Doc[]
    ///     {
    ///         "first line",
    ///         "second line",
    ///         "third line"
    ///     }.Separated(Doc.LineBreak).Indented(4)
    /// );
    /// Console.WriteLine(doc);
    /// // Output:
    /// // leading text     first line
    /// //                  second line
    /// //                  third line
    /// </code>
    /// </example>
    /// <param name="indentation">
    /// The amount of
    /// </param>
    public Document<T> Indented(int indentation)
        => new WhiteSpaceDocument<T>(indentation).Append(Aligned());

    /// <summary>
    /// Sets the nesting level to the current column plus
    /// <paramref name="amount"/>.
    /// </summary>
    /// <remarks>
    /// <c>doc.Hanging(amt)</c> is equivalent to
    /// <c>doc.Nested(amount).Aligned()</c>.
    /// </remarks>
    /// <example name="Hanging example">
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "leading text ",
    ///     new Doc[]
    ///     {
    ///         "first line",
    ///         "second line",
    ///         "third line"
    ///     }.Separated(Doc.LineBreak).Hanging(4)
    /// );
    /// Console.WriteLine(doc);
    /// // Output:
    /// // leading text first line
    /// //                  second line
    /// //                  third line
    /// </code>
    /// </example>
    public Document<T> Hanging(int amount)
        => Nested(amount).Aligned();

    /// <summary>
    /// Apply an annotation to the current <see cref="Document{T}"/>.
    /// The annotation will be passed to the <see cref="IDocumentRenderer{T}"/>.
    /// </summary>
    /// <param name="value">The annotation</param>
    /// <returns>
    /// A copy of the current <see cref="Document{T}"/> with an annotation applied.
    /// </returns>
    public Document<T> Annotated(T value)
        => new AnnotatedDocument<T>(value, this);

    /// <summary>
    /// Apply a function to all the annotations in the current document.
    /// </summary>
    /// <remarks>
    /// Typically this method is less efficient than
    /// <see cref="DocumentRendererExtensions.MapAnnotations{T, U}(IDocumentRenderer{U}, Func{T, U})"/>,
    /// because this method has to consider all the possible ways the document
    /// could be laid out, including layouts which would be discarded.
    /// </remarks>
    /// <param name="selector">The function to apply to the annotations.</param>
    /// <returns>
    /// A document with all of the annotations replaced
    /// with the return value of <paramref name="selector"/>
    /// </returns>
    public Document<U> MapAnnotations<U>(Func<T, U> selector)
        => MapAnnotationsCore(x => new[] { selector(x) });

    /// <summary>
    /// Apply a function to all the annotations in the current document.
    /// If the function returns multiple annotations,
    /// the annotations are added in a left-to-right fashion.
    /// </summary>
    /// <param name="selector">The function to apply to the annotations.</param>
    /// <returns>
    /// A document with all of the annotations replaced
    /// with the return values of <paramref name="selector"/>
    /// </returns>
    public Document<U> MapAnnotations<U>(Func<T, IEnumerable<U>> selector)
        => MapAnnotationsCore(selector);

    /// <summary>
    /// Apply a function to all the annotations in the current document.
    /// </summary>
    /// <remarks>
    /// Typically this method is less efficient than
    /// <see cref="DocumentRendererExtensions.MapAnnotations{T, U}(IDocumentRenderer{U}, Func{T, U})"/>,
    /// because this method has to consider all the possible ways the document
    /// could be laid out, including layouts which would be discarded.
    /// </remarks>
    /// <param name="selector">The function to apply to the annotations.</param>
    /// <returns>
    /// A document with all of the annotations replaced
    /// with the return value of <paramref name="selector"/>
    /// </returns>
    public Document<U> Select<U>(Func<T, U> selector)
        => MapAnnotations(selector);

    /// <summary>
    /// Represents a <see cref="Document{T}"/> containing no text.
    /// </summary>
    /// <example name="Empty with LineBreak">
    /// <see cref="Empty"/> is equivalent to <c>Doc.FromString("")</c>.
    /// This means that the document still occupies vertical space:
    /// <code doctest="true">
    /// var doc = new Doc[] { "abc", Doc.Empty, "def" }
    ///     .Separated(Doc.LineBreak);
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc
    /// // 
    /// // def
    /// </code>
    /// </example>
    /// <returns>A <see cref="Document{T}"/> containing no text.</returns>
    public static Document<T> Empty { get; } = new EmptyDocument<T>();

    /// <summary>
    /// A <see cref="Document{T}"/> which always creates a
    /// line break, even when <see cref="Grouped"/>.
    /// This document cannot be flattened.
    /// You should probably use <see cref="LineBreak"/>.
    /// </summary>
    /// <example name="HardLineBreak grouped">
    /// <see cref="HardLineBreak"/> negates the effect
    /// of <see cref="Grouped"/>, even when there are
    /// other line breaks in the group.
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "abc", Doc.LineBreak,
    ///     "def", Doc.HardLineBreak,
    ///     "ghi"
    /// ).Grouped();
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc
    /// // def
    /// // ghi
    /// </code>
    /// </example>
    public static Document<T> HardLineBreak { get; } = new LineDocument<T>();

    /// <summary>
    /// <para>
    /// A <see cref="Document{T}"/> which advances to the next line
    /// and indents to the current nesting level.
    /// </para>
    /// 
    /// <para>
    /// When flattened, <see cref="ZeroWidthLineBreak"/> behaves like
    /// <see cref="Empty"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <see cref="ZeroWidthLineBreak"/> is like <see cref="LineBreak"/>,
    /// except it behaves like <see cref="Empty"/> if the line break
    /// is undone by <see cref="Grouped"/>.
    /// </remarks>
    /// <remarks>
    /// <see cref="ZeroWidthLineBreak"/> is equivalent to
    /// <c>Document&lt;T&gt;.LineOr("")</c>.
    /// </remarks>
    /// <example name="ZeroWidthLineBreak ungrouped">
    /// The default behaviour of <see cref="ZeroWidthLineBreak"/>
    /// is to start a new line.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.ZeroWidthLineBreak)
    ///     .Append("def");
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    /// <example name="ZeroWidthLineBreak grouped - fits">
    /// When the <see cref="ZeroWidthLineBreak"/> is <see cref="Grouped"/>,
    /// the layout algorithm tries to compress the line break to nothing.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.ZeroWidthLineBreak)
    ///     .Append("def")
    ///     .Grouped();
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abcdef
    /// </code>
    /// </example>
    /// <example name="ZeroWidthLineBreak grouped - doesn't fit">
    /// When the <see cref="ZeroWidthLineBreak"/> is <see cref="Grouped"/>,
    /// but the page width is too narrow to fit the text into a single line,
    /// the text is broken at the <see cref="ZeroWidthLineBreak"/>.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.ZeroWidthLineBreak)
    ///     .Append("def")
    ///     .Grouped();
    /// Console.WriteLine(doc.ToString(4));
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    /// <seealso cref="LineBreak"/>
    /// <seealso cref="LineBreakOr"/>
    public static Document<T> ZeroWidthLineBreak { get; } = LineBreakOr("");

    /// <summary>
    /// <para>
    /// A <see cref="Document{T}"/> which advances to the next line
    /// and indents to the current nesting level.
    /// </para>
    /// 
    /// <para>
    /// When flattened, <see cref="LineBreak"/> is displayed as a
    /// single space.
    /// </para>
    /// </summary>
    /// <remarks>
    /// By default, <see cref="LineBreak"/> starts a new line.
    /// However, if the line break is undone by <see cref="Grouped"/>,
    /// the line break is rendered as a single space.
    /// </remarks>
    /// <remarks>
    /// <see cref="LineBreak"/> is equivalent to
    /// <c>Document&lt;T&gt;.LineOr(" ")</c>.
    /// </remarks>
    /// <example name="LineBreak ungrouped">
    /// The default behaviour of <see cref="LineBreak"/>
    /// is to start a new line.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.LineBreak)
    ///     .Append("def");
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    /// <example name="LineBreak grouped - fits">
    /// When the <see cref="LineBreak"/> is <see cref="Grouped"/>,
    /// the layout algorithm tries to compress the line break to a single space.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.LineBreak)
    ///     .Append("def")
    ///     .Grouped();
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc def
    /// </code>
    /// </example>
    /// <example name="LineBreak grouped - doesn't fit">
    /// When the <see cref="LineBreak"/> is <see cref="Grouped"/>,
    /// but the page width is too narrow to fit the text into a single line,
    /// the text is broken at the <see cref="LineBreak"/>.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.LineBreak)
    ///     .Append("def")
    ///     .Grouped();
    /// Console.WriteLine(doc.ToString(4));
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    /// <seealso cref="LineBreakOr"/>
    public static Document<T> LineBreak { get; } = LineBreakOr(" ");

    /// <summary>
    /// <para>
    /// Creates a <see cref="Document{T}"/> which advances
    /// to the next line and indents to the current nesting level.
    /// </para>
    /// 
    /// <para>
    /// When flattened, the <paramref name="ifFlattened"/> text
    /// is displayed instead.
    /// </para>
    /// </summary>
    /// <remarks>
    /// By default, <c>LineBreakOr(ifFlattened)</c> starts a new line.
    /// However, if the line break is undone by <see cref="Grouped"/>,
    /// the line break is rendered as <paramref name="ifFlattened"/>.
    /// </remarks>
    /// <example name="LineBreakOr ungrouped">
    /// The default behaviour of <see cref="LineBreakOr"/>
    /// is to start a new line.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.LineBreakOr("|"))
    ///     .Append("def");
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    /// <example name="LineBreakOr grouped - fits">
    /// When the document is <see cref="Grouped"/>,
    /// the layout algorithm tries to render the line break
    /// as <paramref name="ifFlattened"/>.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.LineBreakOr("|"))
    ///     .Append("def")
    ///     .Grouped();
    /// Console.WriteLine(doc);
    /// // Output:
    /// // abc|def
    /// </code>
    /// </example>
    /// <example name="LineBreakOr grouped - doesn't fit">
    /// When the <see cref="LineBreakOr"/> is <see cref="Grouped"/>,
    /// but the page width is too narrow to fit the text into a single line,
    /// the text is broken at the line break.
    /// <code doctest="true">
    /// var doc = Doc.FromString("abc")
    ///     .Append(Doc.LineBreakOr("|"))
    ///     .Append("def")
    ///     .Grouped();
    /// Console.WriteLine(doc.ToString(4));
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    /// <seealso cref="LineBreak"/>
    public static Document<T> LineBreakOr(Document<T> ifFlattened)
    {
        ArgumentNullException.ThrowIfNull(ifFlattened);
        return new AlternativeDocument<T>(
            HardLineBreak,
            ifFlattened
        );
    }

    /// <summary>
    /// A "soft line break". Behaves like <c>FromString(" ")</c>
    /// if the resulting output fits the page; otherwise, this behaves
    /// like <see cref="LineBreak"/>.
    /// </summary>
    /// <example name="LineBreakHint when fits">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.Concat("abc", Doc.LineBreakHint, "def"));
    /// // Output:
    /// // abc def
    /// </code>
    /// </example>
    /// <example name="LineBreakHint when too wide">
    /// <code doctest="true">
    /// var doc = Doc.Concat("abc", Doc.LineBreakHint, "def");
    /// Console.WriteLine(doc.ToString(5));
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    public static Document<T> LineBreakHint { get; }
        = new ChoiceDocument<T>(" ", HardLineBreak);

    /// <summary>
    /// A "soft line break". Behaves like <see cref="Empty"/>
    /// if the resulting output fits the page; otherwise, this behaves
    /// like <see cref="LineBreak"/>.
    /// </summary>
    /// <example name="ZeroWidthLineBreakHint when fits">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.Concat("abc", Doc.ZeroWidthLineBreakHint, "def"));
    /// // Output:
    /// // abcdef
    /// </code>
    /// </example>
    /// <example name="ZeroWidthLineBreakHint when too wide">
    /// <code doctest="true">
    /// var doc = Doc.Concat("abc", Doc.ZeroWidthLineBreakHint, "def");
    /// Console.WriteLine(doc.ToString(5));
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    public static Document<T> ZeroWidthLineBreakHint { get; }
        = new ChoiceDocument<T>("", HardLineBreak);

    /// <summary>
    /// A "soft line break". Behaves like <paramref name="ifFits"/>
    /// if the resulting output fits the page; otherwise, this behaves
    /// like <see cref="LineBreak"/>.
    /// </summary>
    /// <example name="ZeroWidthLineBreakHint when fits">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.Concat("abc", Doc.LineBreakHintOr("|"), "def"));
    /// // Output:
    /// // abc|def
    /// </code>
    /// </example>
    /// <example name="ZeroWidthLineBreakHint when too wide">
    /// <code doctest="true">
    /// var doc = Doc.Concat("abc", Doc.LineBreakHintOr("|"), "def");
    /// Console.WriteLine(doc.ToString(5));
    /// // Output:
    /// // abc
    /// // def
    /// </code>
    /// </example>
    /// <param name="ifFits">
    /// The document to render if there's enough space
    /// </param>
    /// <returns>
    /// A <see cref="Document{T}"/> which behaves like
    /// <paramref name="ifFits"/> if the resulting output
    /// fits the page but allows the layout engine to break the line
    /// if it doesn't.
    /// </returns>
    public static Document<T> LineBreakHintOr(Document<T> ifFits)
    {
        ArgumentNullException.ThrowIfNull(ifFits);
        return new ChoiceDocument<T>(ifFits, HardLineBreak);
    }

    /// <summary>
    /// <para>
    /// Creates a <see cref="Document{T}"/> containing
    /// the specified text.
    /// </para>
    ///
    /// <para>
    /// Any line breaks (<c>'\n'</c> characters) in the
    /// <paramref name="value"/> will be converted to
    /// <see cref="LineBreak"/>s.
    /// </para>
    /// </summary>
    /// <example name="FromString">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.FromString("abc"));
    /// // Output:
    /// // abc
    /// </code>
    /// </example>
    /// <example name="FromString with line breaks">
    /// If the <paramref name="value"/> contains line breaks,
    /// they will be converted to <see cref="LineBreak"/>s.
    /// If the <see cref="Document{T}"/> is <see cref="Grouped"/>
    /// then the line breaks are liable to be flattened.
    /// <code doctest="true">
    /// Console.WriteLine(Doc.FromString("abc\ndef").Grouped());
    /// // Output:
    /// // abc def
    /// </code>
    /// </example>
    /// <param name="value">
    /// The text from which to create the <see cref="Document{T}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Document{T}"/> representing the text
    /// containted in <paramref name="value"/>.
    /// </returns>
    public static Document<T> FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var pieces = value
            .SplitLines()
            .Select(l => new TextDocument<T>(l))
            .Intersperse(LineBreak);
        return Concat(pieces);
    }

    /// <summary>
    /// <para>
    /// Creates a <see cref="Document{T}"/> containing
    /// the specified text.
    /// </para>
    ///
    /// <para>
    /// The <paramref name="value"/> MUST NOT contain
    /// newline characters. This precondition is not checked.
    /// It is your responsibility to ensure that there are no
    /// <c>'\n'</c>s in the input.
    /// </para>
    /// 
    /// <para>
    /// You should probably use <see cref="FromString"/> instead.
    /// </para>
    /// </summary>
    /// <param name="value">
    /// The text from which to create the <see cref="Document{T}"/>.
    /// This value MUST NOT contain line breaks.
    /// </param>
    /// <returns>
    /// A <see cref="Document{T}"/> representing the text
    /// containted in <paramref name="value"/>.
    /// </returns>
    /// <seealso cref="FromString"/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Document<T> UnsafeFromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new TextDocument<T>(value.AsMemory());
    }

    /// <summary>
    /// Creates a document containing a <see cref="Box{T}"/>.
    /// </summary>
    /// <param name="box">
    /// The <see cref="Box{T}"/> from which to create the
    /// <see cref="Document{T}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Document{T}"/> representing the text
    /// containted in <paramref name="box"/>.
    /// </returns>
    public static Document<T> FromBox(Box<T> box)
    {
        ArgumentNullException.ThrowIfNull(box);
        return new BoxDocument<T>(box);
    }

    /// <summary>
    /// Inserts <see cref="LineBreakHint"/>s
    /// between words, so that the text is broken into multiple
    /// lines when it is too wide for the page.
    /// </summary>
    public static Document<T> Reflow(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        IEnumerable<Document<T>> Iterator()
        {
            var docLength = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]) && i > docLength)
                {
                    yield return new TextDocument<T>(text.AsMemory()[docLength..i]);
                    docLength = i + 1;
                }
            }
            yield return new TextDocument<T>(text.AsMemory()[docLength..]);
        }
        return Concat(Iterator().Intersperse(LineBreakHint));
    }

    /// <summary>
    /// Returns a new <see cref="Document{T}"/> representing
    /// the contents of all of <paramref name="documents"/>
    /// concatenated together.
    /// </summary>
    /// <example name="Concat example">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.Concat("abc", "def", "ghi"));
    /// // Output:
    /// // abcdefghi
    /// </code>
    /// </example>
    /// <param name="documents">The documents to concatenate</param>
    /// <returns>
    /// A new <see cref="Document{T}"/> representing
    /// the contents of all of <paramref name="documents"/>
    /// concatenated together.
    /// </returns>
    public static Document<T> Concat(params Document<T>[] documents)
    {
        ArgumentNullException.ThrowIfNull(documents);
        return Concat(documents.AsEnumerable());
    }

    /// <summary>See <see cref="Concat(Document{T}[])"/>.</summary>
    public static Document<T> Concat(IEnumerable<Document<T>> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);
        return documents.Aggregate(Empty, (acc, d) => acc.Append(d));
    }

    /// <summary>
    /// Returns a new <see cref="Document{T}"/> representing
    /// the content of <paramref name="left"/> concatenated with the
    /// content of <paramref name="right"/>.
    /// </summary>
    /// <remarks>
    /// <c>x + y</c> is equivalent to <c>x.Append(y)</c>.
    /// </remarks>
    /// <example name="`+` example">
    /// <code doctest="true">
    /// Console.WriteLine(Doc.FromString("abc") + Doc.FromString("def"));
    /// // Output:
    /// // abcdef
    /// </code>
    /// </example>
    /// <param name="left">
    /// The document to append <paramref name="right"/> to
    /// </param>
    /// <param name="right">
    /// The document to append to <paramref name="left"/>
    /// </param>
    /// <returns>
    /// A new <see cref="Document{T}"/> representing
    /// the content of <paramref name="left"/> concatenated with the
    /// content of <paramref name="right"/>.
    /// </returns>
    /// <seealso cref="Append"/>
    [SuppressMessage("design", "CA2225", Justification = "It's called Append")]  // "Provide a method named 'Add' as a friendly alternate for operator op_Addition"
    public static Document<T> operator +(Document<T> left, Document<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.Append(right);
    }

    /// <summary>
    /// Implicitly convert a <see cref="string"/> to a <see cref="Document{T}"/>.
    /// </summary>
    /// <remarks>
    /// This conversion is equivalent to <see cref="FromString"/>.
    /// </remarks>
    /// <example name="Implicit conversion">
    /// This conversion is especially useful when used with
    /// methods which accept a <see cref="Document{T}"/>,
    /// such as <see cref="Concat(Document{T}[])"/>.
    /// <code>
    /// Console.WriteLine(Doc.Concat("abc", "def"));
    /// // Output:
    /// // abcdef
    /// </code>
    /// </example>
    /// <param name="value">
    /// The text from which to create the <see cref="Document{T}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Document{T}"/> representing the text
    /// containted in <paramref name="value"/>.
    /// </returns>
    public static implicit operator Document<T>(string value)
        => FromString(value);

    /// <summary>
    /// Implicitly convert a <see cref="Box{T}"/> to a
    /// <see cref="Document{T}"/>.
    /// </summary>
    /// <remarks>
    /// This conversion is equivalent to <see cref="FromBox"/>.
    /// </remarks>
    /// <param name="value">
    /// The <see cref="Box{T}"/> from which to create the
    /// <see cref="Document{T}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Document{T}"/> representing the text
    /// containted in <paramref name="value"/>.
    /// </returns>
    public static implicit operator Document<T>(Box<T> value)
        => FromBox(value);

    internal abstract Document<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector);
    internal abstract ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken);
}
