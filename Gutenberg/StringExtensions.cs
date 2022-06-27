namespace Gutenberg;

internal static class StringExtensions
{
    public static IEnumerable<ReadOnlyMemory<char>> SplitLines(this string str)
    {
        var seenLength = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] == '\n')
            {
                yield return str.AsMemory()[seenLength..i];
                seenLength = i + 1;
            }
        }
        yield return str.AsMemory()[seenLength..];
    }
}
