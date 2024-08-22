Gutenberg
=========

A library for laying out text, based on [Phil Wadler's pretty printing algorithm](https://homepages.inf.ed.ac.uk/wadler/papers/prettier/prettier.pdf).


Installing
----------

Gutenberg is [available on Nuget](https://www.nuget.org/packages/Gutenberg). API docs are hosted [on my website](https://www.benjamin.pizza/Gutenberg).


Tutorial
--------

Gutenberg's core object is called `Document<T>`. It represents a textual document which can be laid out in a variety of ways.

You can create a document from a string,

```csharp
// Document<T> is intended to be imported under an alias.
// If you're not using annotations (an advanced feature),
// you can set `T` to `object`.
using Doc = Gutenberg.Document<object>;

var doc = Doc.FromString("Hello world!");
```

concatenate documents,

```csharp
var doc2 = doc + " My name is Johannes Gutenberg.";  // strings can be implicitly cast to documents
```

insert line breaks,

```csharp
var doc3 = doc2 + Doc.LineBreak + "Pleased to meet you!";
```

and render the document as a string.

```csharp
Console.WriteLine(doc3.ToString());
// Output:
// Hello world! My name is Johannes Gutenberg.
// Pleased to meet you!
```

You can instruct Gutenberg to try alternative layouts for a document using the `Grouped` method. `Grouped` tells Gutenberg to attempt to _flatten_ the document, collapsing any `LineBreak`s into a single space.

```csharp
Console.WriteLine(doc3.Grouped());
// Output:
// Hello world! My name is Johannes Gutenberg. Pleased to meet you!
```

Gutenberg tries to use its available horizontal space (a default of 80 characters) efficiently. It tries to flatten the groups within a document as long as there is enough horizontal space to do so. If there's an overflow, it falls back on the un-flattened version.

```csharp
// set the page width to 60 characters - too narrow to flatten the LineBreak
Console.WriteLine(doc3.Grouped().ToString(60));
// Output:
// Hello world! My name is Johannes Gutenberg.
// Pleased to meet you!
```

The `Nested` operator controls indentation. If a document gets rendered with line breaks, the line breaks are indented to the document's nesting level.

```csharp
Console.WriteLine(doc3.Nested(4).Grouped().ToString(60));
// Output:
// Hello world! My name is Johannes Gutenberg.
//     Pleased to meet you!
```

You can place groups inside each other, to give Gutenberg multiple options to layout a document as efficiently as possible. Here's an example of a recursive function to pretty-print a tree which uses nested groups to produce a flexible layout.

```csharp
record Tree(string Name, params Tree[] Children) : IPrettyPrintable<object>
{
    public Doc PrettyPrint()
    {
        var children = Children.Length == 0
            ? Doc.Empty
            : Doc.ZeroWidthLineBreak
                .Append(Doc.Concat(
                    Children
                        .Select(c => c.PrettyPrint())
                        .Separated("," + Doc.LineBreak)
                ))
                .Nested(2)
                .Between("[", Doc.ZeroWidthLineBreak + "]");
        return Doc.Concat(Name).Append(children).Grouped();
    }
}
Console.WriteLine(exampleTree.PrettyPrint().ToString(20))
// Output:
// aaa[
//   bbbbb[ccc, dd],
//   eee,
//   ffff[gg, hhh, ii]
// ]
```

You can read about Gutenberg's internals [on my website](https://www.benjamin.pizza/posts/2024-08-15-prettier-happier-more-imperative.html).
