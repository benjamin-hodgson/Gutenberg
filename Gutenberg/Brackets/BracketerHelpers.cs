namespace Gutenberg.Brackets;

internal static class BracketHelpers
{
    public static Document<T> GroupIf<T>(this Document<T> doc, bool condition)
        => condition ? doc.Grouped() : doc;

    public static Document<T> NestBy<T>(this Document<T> doc, int? nesting)
        => nesting == 0
            ? doc
            : nesting.HasValue
                ? doc.Nested(nesting.Value)
                : doc.Nested();
}
