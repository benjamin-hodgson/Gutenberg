namespace Gutenberg;

internal readonly struct StringSlice
{
    public string String { get; }

    public int StartIndex { get; }

    public int Length { get; }

    public StringSlice(string str, int startIndex, int length)
    {
        String = str;
        StartIndex = startIndex;
        Length = length;
    }

    public StringSlice Slice(int startIndex, int length)
        => new(String, StartIndex + startIndex, length);

    public ReadOnlyMemory<char> AsMemory()
        => String.AsMemory(StartIndex, Length);
}
