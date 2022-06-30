namespace Gutenberg;

internal interface IStackItem<T> { }

internal record ChoicePoint<T>(
    Document<T> Fallback,
    int NestingLevel,
    int BufferedInstructionCount,
    int LineTextLength,
    bool Flatten,
    bool CanBacktrack,
    int ResumeAt  // stack location
) : IStackItem<T>;

internal class SetNestingLevel<T> : IStackItem<T>
{
    // 128 because default page width is 80
    private static readonly SetNestingLevel<T>[] _cache = new SetNestingLevel<T>[128];

    public int NestingLevel { get; }

    private SetNestingLevel(int nestingLevel)
    {
        NestingLevel = nestingLevel;
    }

    public void Deconstruct(out int nestingLevel)
    {
        nestingLevel = NestingLevel;
    }

    public static SetNestingLevel<T> Create(int nestingLevel)
    {
        if (nestingLevel < _cache.Length)
        {
            if (_cache[nestingLevel] == null)
            {
                _cache[nestingLevel] = new(nestingLevel);
            }
            return _cache[nestingLevel];
        }
        return new(nestingLevel);
    }
}

internal record PopAnnotation<T> : IStackItem<T>
{
    private PopAnnotation() { }
    public static PopAnnotation<T> Instance { get; }
        = new PopAnnotation<T>();
}

internal record EndFlatten<T> : IStackItem<T>
{
    private EndFlatten() { }
    public static EndFlatten<T> Instance { get; }
        = new EndFlatten<T>();
}
