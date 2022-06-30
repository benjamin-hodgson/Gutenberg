namespace Gutenberg;

internal interface IStackItem<T> { }

internal class ChoicePoint<T> : IStackItem<T>
{
    public Document<T> Fallback { get; }
    public int NestingLevel { get; }
    public int BufferedInstructionCount { get; }
    public int LineTextLength { get; }
    public bool Flatten { get; }
    public bool CanBacktrack { get; }
    // a stack location. gets updated by
    // LayoutEngine upon popping a ChoicePoint
    public int ResumeAt { get; set; }

    public ChoicePoint(
        Document<T> fallback,
        int nestingLevel,
        int bufferedInstructionCount,
        int lineTextLength,
        bool flatten,
        bool canBacktrack,
        int resumeAt
    )
    {
        Fallback = fallback;
        NestingLevel = nestingLevel;
        BufferedInstructionCount = bufferedInstructionCount;
        LineTextLength = lineTextLength;
        Flatten = flatten;
        CanBacktrack = canBacktrack;
        ResumeAt = resumeAt;
    }
}

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

internal class PopAnnotation<T> : IStackItem<T>
{
    private PopAnnotation() { }
    public static PopAnnotation<T> Instance { get; }
        = new PopAnnotation<T>();
}

internal class EndFlatten<T> : IStackItem<T>
{
    private EndFlatten() { }
    public static EndFlatten<T> Instance { get; }
        = new EndFlatten<T>();
}
