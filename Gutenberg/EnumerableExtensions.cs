namespace Gutenberg;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T value)
    {
        var first = true;
        foreach (var x in source)
        {
            if (!first)
            {
                yield return value;
            }
            yield return x;
            first = false;
        }
    }
}
