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
    private readonly object? _object;
    private readonly int _offset;
    private readonly int _length;

    private LayoutInstruction(object? obj, int offset, int length)
    {
        _object = obj;
        _offset = offset;
        _length = length;
    }

    public bool IsNewLine => ReferenceEquals(_object, NewLineInstruction.Instance);
    public bool IsText => _offset >= 0 && _length >= 0;

    public LayoutInstructionType GetInstructionType()
    {
        if (IsNewLine)
        {
            return LayoutInstructionType.NewLine;
        }
        if (ReferenceEquals(_object, PopAnnotationInstruction.Instance))
        {
            return LayoutInstructionType.PopAnnotation;
        }
        if (_offset < 0 && _length < 0)
        {
            return LayoutInstructionType.PushAnnotation;
        }
        if (_offset < 0)
        {
            // _length >= 0
            return LayoutInstructionType.WhiteSpace;
        }
        if (IsText)
        {
            if (_object is not string)
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
        => ((string)_object!).AsMemory(_offset, _length);

    public int GetWhitespaceAmount()
        => _length;

    public T GetAnnotation()
        // nullability: This should only be called if
        // GetInstructionType returned PushAnnotation.
        // Any nullness will be inherited from the
        // T with which this struct was instantiated.
        => (T)_object!;

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
