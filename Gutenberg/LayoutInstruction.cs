namespace Gutenberg;

internal abstract record LayoutInstruction<T>;
internal record TextInstruction<T>(ReadOnlyMemory<char> Text) : LayoutInstruction<T>;
internal record WhiteSpaceInstruction<T>(int Amount) : LayoutInstruction<T>;
internal record NewLineInstruction<T> : LayoutInstruction<T>
{
    private NewLineInstruction() { }
    public static NewLineInstruction<T> Instance { get; }
        = new NewLineInstruction<T>();
}
internal record AnnotationInstruction<T>(T Value) : LayoutInstruction<T>;
internal record PopAnnotationInstruction<T> : LayoutInstruction<T>
{
    private PopAnnotationInstruction() { }
    public static PopAnnotationInstruction<T> Instance { get; }
        = new PopAnnotationInstruction<T>();
}
