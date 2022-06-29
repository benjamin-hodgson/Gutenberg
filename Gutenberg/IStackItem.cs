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

internal record SetNestingLevel<T>(int NestingLevel) : IStackItem<T>;

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
