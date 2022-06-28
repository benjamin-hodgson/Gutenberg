namespace Gutenberg.Rendering;

/// <summary>
/// An <see cref="IDocumentRenderer{T}"/> which writes formatted text to the <see cref="Console"/>.
/// </summary>
public class ConsoleDocumentRenderer : IDocumentRenderer<ConsoleFormattingAnnotation>
{
    private readonly Stack<(ConsoleColor previousColor, ConsoleFormattingAnnotation instr)> _stack = new();

    /// <inheritdoc cref="IDocumentRenderer{T}.NewLine"/>
    public ValueTask NewLine(CancellationToken cancellationToken = default)
    {
        Console.WriteLine();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.Text"/>
    public ValueTask Text(ReadOnlyMemory<char> memory, CancellationToken cancellationToken = default)
    {
        Console.Out.Write(memory.Span);
        return ValueTask.CompletedTask;
    }
    
    /// <inheritdoc cref="IDocumentRenderer{T}.WhiteSpace"/>
    public ValueTask WhiteSpace(int amount, CancellationToken cancellationToken = default)
    {
        Console.Write(new string(' ', amount));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.PushAnnotation"/>
    public ValueTask PushAnnotation(ConsoleFormattingAnnotation value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        switch (value)
        {
            case ConsoleFormattingAnnotation.ForegroundColor(var colour):
                _stack.Push((Console.ForegroundColor, value));
                Console.ForegroundColor = colour;
                break;
            case ConsoleFormattingAnnotation.BackgroundColor(var colour):
                _stack.Push((Console.BackgroundColor, value));
                Console.BackgroundColor = colour;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Unknown {nameof(ConsoleFormattingAnnotation)}");
        }
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="IDocumentRenderer{T}.PopAnnotation"/>
    public ValueTask PopAnnotation(CancellationToken cancellationToken = default)
    {
        var (previousColour, instr) = _stack.Pop();
        switch (instr)
        {
            case ConsoleFormattingAnnotation.ForegroundColor:
                Console.ForegroundColor = previousColour;
                break;
            case ConsoleFormattingAnnotation.BackgroundColor:
                Console.BackgroundColor = previousColour;
                break;
        }
        return ValueTask.CompletedTask;
    }
}
