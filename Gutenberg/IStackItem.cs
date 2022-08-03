namespace Gutenberg;

internal interface IStackItem<T> { }

internal class ChoicePoint<T> : IStackItem<T>
{
    public Document<T>? Fallback { get; set; }
    public int NestingLevel { get; set; }
    public int LineBufferCount { get; set; }
    public int LineTextLength { get; set; }
    public bool Flatten { get; set; }
    public bool CanBacktrack { get; set; }
    public int BufferUntilDeIndent { get; set; }
    // a stack location. gets updated by
    // LayoutEngine upon popping a ChoicePoint
    public int ResumeAt { get; set; }
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
