namespace Gutenberg;

/// <summary>
/// Options for laying out a <see cref="Document{T}"/>.
/// </summary>
/// <param name="PageWidth">
/// The configured page width. Null for unlimited page width
/// </param>
/// <param name="LayoutMode">The layout mode</param>
/// <param name="DefaultNesting">
/// The default indentation level for calls
/// to <see cref="Document{T}.Nested()"/>
/// </param>
/// <param name="StripTrailingWhitespace">
/// Whether to strip whitespace from the end of each line
/// </param>
public record LayoutOptions(
    PageWidthOptions? PageWidth,
    LayoutMode LayoutMode,
    int DefaultNesting,
    bool StripTrailingWhitespace
)
{
    /// <summary>The default <see cref="LayoutOptions"/>.</summary>
    public static LayoutOptions Default { get; }
        = new(new PageWidthOptions(), LayoutMode.Default, 4, true);
}

/// <summary>
/// Information about the available width for layout
/// </summary>
/// <param name="PageWidth">
/// The total width of the page in characters
/// </param>
/// <param name="PrintableRatio">
/// The percentage of the total page width that can have
/// printed (non-indentation) characters on it.
/// Also known as the ribbon width.
/// </param>
public record PageWidthOptions(
    int PageWidth = 80,
    double PrintableRatio = 1
);

/// <summary>The layout mode.</summary>
public enum LayoutMode
{
    /// <summary>
    /// The default layout mode, with one line of lookahead
    /// </summary>
    Default,

    /// <summary>
    /// A fast-and-ugly layout mode. Tells the layout engine
    /// not to collapse any line breaks and to ignore all
    /// indentation and annotations. The resulting text contains
    /// fewer characters than the <see cref="Default"/> mode,
    /// so may be appropriate for machine-readable output in
    /// indentation-insensitive contexts.
    /// </summary>
    /// <example name="LayoutMode.Simple">
    /// <code doctest="true">
    /// var doc = Doc.Concat(
    ///     "abc", Doc.LineBreak,
    ///     Doc.Concat("def", Doc.LineBreak, "ghi").Nested()
    /// );
    /// var options = LayoutOptions.Default with
    /// {
    ///    LayoutMode = LayoutMode.Simple
    /// };
    /// Console.WriteLine(doc.ToString(options));
    /// // Output:
    /// // abc
    /// // def
    /// // ghi
    /// </code>
    /// </example>
    Simple,

    /// <summary>
    /// <para>
    /// A "smart" layout mode, which can produce better layouts
    /// than <see cref="Default"/> under certain circumstances,
    /// at the expense of some performance.
    /// </para>
    /// <para>
    /// The <see cref="Default"/> layout mode commits to rendering a
    /// <see cref="Document{T}"/> in a certain way if the rest of the
    /// current line fits within the page width. In other words,
    /// the <see cref="Default"/> layout mode has up to one line of
    /// lookahead.
    /// </para>
    /// <para>
    /// However, this lookahead strategy can sometimes produce inefficient
    /// layouts when rendering <see cref="Document{T}.Aligned"/>
    /// blocks. If the first line of an aligned block fits within
    /// the page width, the default layout mode commits to rendering the
    /// whole aligned block indented to the current column, even if that
    /// would cause a line later on in the block to overflow.
    /// </para>
    /// <para>
    /// The <see cref="Smart"/> layout mode instructs the layout engine
    /// to look ahead further than a single line when rendering an
    /// aligned block; instead it looks ahead to the first de-indent.
    /// This means the smart layout algorithm might take a
    /// <see cref="Document{T}.LineBreakHint"/>s preceding an aligned
    /// block, if doing so would prevent a line in the aligned block
    /// from overflowing.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This layout mode generally performs worse (more backtracking)
    /// than the default layout mode, but can produce better layouts
    /// for documents featuring <see cref="Document{T}.Aligned"/> blocks.
    /// </remarks>
    /// <example name="Smart layout">
    /// In this example, the second line of the aligned block causes an
    /// overflow in the <see cref="Default"/> layout mode. However,
    /// <see cref="Smart"/> layout mode decides to take the line break hint
    /// on the first line in order to prevent the overflow.
    /// <code doctest="true">
    /// var doc = Doc.LineBreakHint
    ///     + new Doc[] { "aligned block", "containing a long line after the first" }
    ///         .Separated(Doc.LineBreak)
    ///         .Aligned();
    ///
    /// var defaultOptions = LayoutOptions.Default with { PageWidth = new(50) };
    /// Console.WriteLine(("Default layout mode:" + doc).ToString(defaultOptions));
    /// Console.WriteLine();
    ///
    /// var smartOptions = defaultOptions with { LayoutMode = LayoutMode.Smart };
    /// Console.WriteLine(("Smart layout mode:" + doc).ToString(smartOptions));
    /// // Output:
    /// // Default layout mode: aligned block
    /// //                      containing a long line after the first
    /// //
    /// // Smart layout mode:
    /// // aligned block
    /// // containing a long line after the first
    /// </code>
    /// </example>
    Smart
}
