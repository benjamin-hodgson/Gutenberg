using static Gutenberg.Tests.DocumentTestUtil;

using Doc = Gutenberg.Document<object>;

namespace Gutenberg.Tests;

public class TreeTests
{
    private static readonly Tree _tree = new(
        "aaa",
        new Tree("bbbbb", new Tree("ccc"), new("dd")),
        new("eee"),
        new("ffff", new Tree("gg"), new("hhh"), new("ii"))
    );

    [Fact]
    public async Task TestTree_UnlimitedWidth()
    {
        // spill nothing
        var expected = @"aaa[bbbbb[ccc, dd], eee, ffff[gg, hhh, ii]]";
        await TestDocument(
            expected,
            _tree.PrettyPrint(),
            LayoutOptions.Default with { PageWidth = null }
        );
    }

    [Fact]
    public async Task TestTree_MediumWidth()
    {
        // spill the top level
        var expected = @"aaa[
  bbbbb[ccc, dd],
  eee,
  ffff[gg, hhh, ii]
]";
        await TestDocument(
            expected,
            _tree.PrettyPrint(),
            20
        );
    }

    [Fact]
    public async Task TestTree_Narrow()
    {
        // spill everything
        var expected = @"aaa[
  bbbbb[
    ccc,
    dd
  ],
  eee,
  ffff[
    gg,
    hhh,
    ii
  ]
]";
        await TestDocument(
            expected,
            _tree.PrettyPrint(),
            10
        );
    }
}


internal record Tree(string Name, params Tree[] Children) : IPrettyPrintable<object>
{
    // https://homepages.inf.ed.ac.uk/wadler/papers/prettier/prettier.pdf,
    // page 3
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
