using System.Runtime.CompilerServices;

namespace Gutenberg;

internal enum LayoutInstructionType
{
    WhiteSpace,
    Text,
    NewLine,
    PushAnnotation,
    PopAnnotation
}

internal readonly struct LayoutInstruction<T>
{
    // null, -1, >=0: write whitespace
    // string, >=0, >=0: write text
    // x, -1, -1: push annotation x.
    // PopAnnotationInstruction.Instance, _, _: pop annotation
    // NewLineInstruction.Instance, _, _: write newline
    public object? Object { get; }
    public int Offset { get; }
    public int Length { get; }

    private LayoutInstruction(object? obj, int offset, int length)
    {
        Object = obj;
        Offset = offset;
        Length = length;
    }

    public LayoutInstructionType GetInstructionType()
    {
        if (ReferenceEquals(Object, NewLineInstruction.Instance))
        {
            return LayoutInstructionType.NewLine;
        }
        if (ReferenceEquals(Object, PopAnnotationInstruction.Instance))
        {
            return LayoutInstructionType.PopAnnotation;
        }
        if (Offset < 0 && Length < 0)
        {
            return LayoutInstructionType.PushAnnotation;
        }
        if (Offset < 0)
        {
            // Length >= 0
            return LayoutInstructionType.WhiteSpace;
        }
        if (Offset >= 0 && Length >= 0)
        {
            if (Object is not string)
            {
                ThrowError();
            }
            return LayoutInstructionType.Text;
        }
        return ThrowError();
    }

    public ReadOnlyMemory<char> GetText()
        // nullability: This should only be called if
        // GetInstructionType returned Text
        => ((string)Object!).AsMemory(Offset, Length);

    public int GetWhitespaceAmount()
        => Length;

    public T GetAnnotation()
        // nullability: This should only be called if
        // GetInstructionType returned PushAnnotation.
        // Any nullness will be inherited from the
        // T with which this struct was instantiated.
        => (T)Object!;

    public static LayoutInstruction<T> WhiteSpace(int amount)
        => new(null, -1, amount);

    public static LayoutInstruction<T> Text(StringSlice slice)
        => new(slice.String, slice.StartIndex, slice.Length);

    public static LayoutInstruction<T> PushAnnotation(T value)
         => new(value, -1, -1);

    public static LayoutInstruction<T> PopAnnotation { get; }
        = new(PopAnnotationInstruction.Instance, -1, -1);

    public static LayoutInstruction<T> NewLine { get; }
        = new(NewLineInstruction.Instance, -1, -1);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static LayoutInstructionType ThrowError()
    {
        throw new InvalidOperationException("Invalid LayoutInstruction! Please report this as a bug in Gutenberg");
    }
}

// sentinel values for LayoutInstruction
internal record NewLineInstruction
{
    private NewLineInstruction() { }
    public static NewLineInstruction Instance { get; }
        = new NewLineInstruction();
}
internal record PopAnnotationInstruction
{
    private PopAnnotationInstruction() { }
    public static PopAnnotationInstruction Instance { get; }
        = new PopAnnotationInstruction();
}
