using System.Diagnostics.CodeAnalysis;

namespace Gutenberg.Rendering;

/// <summary>
/// A base class for annotations which the
/// <see cref="ConsoleDocumentRenderer"/> understands
/// </summary>
[SuppressMessage("design", "CA1034")]  // "Do not nest types"
public abstract record ConsoleFormattingAnnotation
{
    private ConsoleFormattingAnnotation() {}

    /// <summary>Set the <see cref="Console.ForegroundColor"/></summary>
    /// <param name="Color">The colour</param>
    public record ForegroundColor(
        ConsoleColor Color
    ) : ConsoleFormattingAnnotation;

    /// <summary>Set the <see cref="Console.BackgroundColor"/></summary>
    /// <param name="Color">The colour</param>
    public record BackgroundColor(
        ConsoleColor Color
    ) : ConsoleFormattingAnnotation;
}
