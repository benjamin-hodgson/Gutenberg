namespace Gutenberg;

internal static class StringExtensions
{
    public static IEnumerable<StringSlice> SplitLines(this string str)
    {
        var seenLength = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] == '\n')
            {
                yield return new StringSlice(str, seenLength, i - seenLength);
                seenLength = i + 1;
            }
        }
        yield return new StringSlice(str, seenLength, str.Length - seenLength);
    }
    public static IEnumerable<StringSlice> SplitWords(this string str)
    {
        var seenLength = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (char.IsWhiteSpace(str[i]) && i > seenLength)
            {
                yield return new StringSlice(str, seenLength, i - seenLength);
                seenLength = i + 1;
            }
        }
        yield return new StringSlice(str, seenLength, str.Length - seenLength);
    }
}
