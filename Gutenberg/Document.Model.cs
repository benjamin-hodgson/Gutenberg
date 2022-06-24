using System.Diagnostics;

namespace Gutenberg;

internal class EmptyDocument<T> : Document<T>
{
    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => Document<U>.Empty;

    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}

[DebuggerDisplay("HardLine")]
internal class LineDocument<T> : Document<T>
{
    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => Document<U>.HardLineBreak;

    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => renderer.NewLine(cancellationToken);
}

internal class WhiteSpaceDocument<T> : Document<T>
{
    public int Amount { get; }

    public WhiteSpaceDocument(int amount)
    {
        Amount = amount;
    }

    public void Deconstruct(out int amount)
    {
        amount = Amount;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new WhiteSpaceDocument<U>(Amount);

    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => renderer.WhiteSpace(Amount, cancellationToken);
}

internal class TextDocument<T> : Document<T>
{
    public ReadOnlyMemory<char> Text { get; }

    public TextDocument(ReadOnlyMemory<char> text)
    {
        Text = text;
    }

    public void Deconstruct(out ReadOnlyMemory<char> text)
    {
        text = Text;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new TextDocument<U>(Text);

    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => renderer.Text(Text, cancellationToken);
}

internal class AppendDocument<T> : Document<T>
{
    public Document<T> Left { get; }
    public Document<T> Right { get; }

    public AppendDocument(Document<T> left, Document<T> right)
    {
        Left = left;
        Right = right;
    }

    public void Deconstruct(out Document<T> left, out Document<T> right)
    {
        left = Left;
        right = Right;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new AppendDocument<U>(
            Left.MapAnnotationsImpl(selector),
            Right.MapAnnotationsImpl(selector)
        );

    internal override async ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
    {
        await Left.RenderSimple(renderer, cancellationToken).ConfigureAwait(false);
        await Right.RenderSimple(renderer, cancellationToken).ConfigureAwait(false);
    }
}

internal class AlternativeDocument<T> : Document<T>
{
    public Document<T> Default { get; }
    public Document<T> IfFlattened { get; }

    public AlternativeDocument(Document<T> @default, Document<T> ifFlattened)
    {
        Default = @default;
        IfFlattened = ifFlattened;
    }

    public void Deconstruct(out Document<T> @default, out Document<T> ifFlattened)
    {
        @default = Default;
        ifFlattened = IfFlattened;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new AlternativeDocument<U>(
            Default.MapAnnotationsImpl(selector),
            IfFlattened.MapAnnotationsImpl(selector)
        );

    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => Default.RenderSimple(renderer, cancellationToken);
}

internal class ChoiceDocument<T> : Document<T>
{
    public Document<T> First { get; }
    public Document<T> Second { get; }

    public ChoiceDocument(Document<T> first, Document<T> second)
    {
        First = first;
        Second = second;
    }

    public void Deconstruct(out Document<T> first, out Document<T> second)
    {
        first = First;
        second = Second;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new ChoiceDocument<U>(
            First.MapAnnotationsImpl(selector),
            Second.MapAnnotationsImpl(selector)
        );

    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => Second.RenderSimple(renderer, cancellationToken);
}

internal class NestedDocument<T> : Document<T>
{
    public int? Indentation { get; }
    public Document<T> Doc { get; }

    public NestedDocument(int? indentation, Document<T> doc)
    {
        Indentation = indentation;
        Doc = doc;
    }

    public void Deconstruct(out int? indentation, out Document<T> doc)
    {
        indentation = Indentation;
        doc = Doc;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new NestedDocument<U>(Indentation, Doc.MapAnnotationsImpl(selector));

    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => Doc.RenderSimple(renderer, cancellationToken);
}

internal class AnnotatedDocument<T> : Document<T>
{
    public T Value { get; }
    public Document<T> Doc { get; }

    public AnnotatedDocument(T value, Document<T> doc)
    {
        Doc = doc;
        Value = value;
    }

    public void Deconstruct(out T value, out Document<T> doc)
    {
        value = Value;
        doc = Doc;
    }
    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => selector(Value)
            .Aggregate(
                Doc.MapAnnotationsImpl(selector),
                (doc, val) => new AnnotatedDocument<U>(val, doc)
            );
    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => Doc.RenderSimple(renderer, cancellationToken);
}

internal class FlattenedDocument<T> : Document<T>
{
    public Document<T> Document { get; }

    public FlattenedDocument(Document<T> document)
    {
        Document = document;
    }

    public void Deconstruct(out Document<T> document)
    {
        document = Document;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new FlattenedDocument<U>(Document.MapAnnotationsImpl(selector));
    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => Document.RenderSimple(renderer, cancellationToken);
}

internal class ColumnInfoDocument<T> : Document<T>
{
    public Func<int, int, Document<T>> Func;

    public ColumnInfoDocument(Func<int, int, Document<T>> func)
    {
        Func = func;
    }

    public void Deconstruct(out Func<int, int, Document<T>> func)
    {
        func = Func;
    }

    internal override Document<U> MapAnnotationsImpl<U>(Func<T, IEnumerable<U>> selector)
        => new ColumnInfoDocument<U>((c, n) => Func(c, n).MapAnnotationsImpl(selector));
    internal override ValueTask RenderSimple(IDocumentRenderer<T> renderer, CancellationToken cancellationToken)
        => Func(0, 0).RenderSimple(renderer, cancellationToken);
}
