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
public record LayoutOptions(
    PageWidthOptions? PageWidth,
    LayoutMode LayoutMode,
    int DefaultNesting
)
{
    /// <summary>The default <see cref="LayoutOptions"/></summary>
    public static LayoutOptions Default { get; }
        = new(new PageWidthOptions(), LayoutMode.Default, 4);
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

/// <summary>The layout mode</summary>
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
    Simple
}
