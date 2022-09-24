namespace Gutenberg;

internal class EmptyBox<T> : Box<T>
{
    public EmptyBox()
        : base(0, 0)
    {
    }

    internal override (Box<T> top, Box<T> bottom) CutHorizontalCore(int lineNumber)
    {
        throw new InvalidOperationException("Tried to cut an empty box. Please report this as a bug in Gutenberg");
    }

    internal override (Box<T> left, Box<T> right) CutVerticalCore(int columnNumber)
    {
        throw new InvalidOperationException("Tried to cut an empty box. Please report this as a bug in Gutenberg");
    }

    internal override Box<T> OverlayCore(Box<T> background) => background;

    internal override ValueTask WriteLine(IDocumentRenderer<T> renderer, int lineNumber, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Tried to write an empty box. Please report this as a bug in Gutenberg");
    }

    internal override Box<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector) => Box<U>.Empty;
}

internal class TransparentBox<T> : Box<T>
{
    public TransparentBox(int width, int height)
        : base(width, height)
    {
    }

    internal override (Box<T> top, Box<T> bottom) CutHorizontalCore(int lineNumber)
        => (
            new TransparentBox<T>(Width, lineNumber),
            new TransparentBox<T>(Width, Height - lineNumber)
        );

    internal override (Box<T> left, Box<T> right) CutVerticalCore(int columnNumber)
        => (
            new TransparentBox<T>(columnNumber, Height),
            new TransparentBox<T>(Width - columnNumber, Height)
        );

    internal override Box<T> OverlayCore(Box<T> background) => background;

    internal override ValueTask WriteLine(IDocumentRenderer<T> renderer, int lineNumber, CancellationToken cancellationToken)
        => renderer.WhiteSpace(Width, cancellationToken);

    internal override Box<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector)
        => Box<U>.Transparent(Width, Height);
}

internal class TextBox<T> : Box<T>
{
    public StringSlice Line { get; }

    public TextBox(StringSlice line)
        : base(line.Length, 1)
    {
        Line = line;
    }

    internal override (Box<T> top, Box<T> bottom) CutHorizontalCore(int lineNumber)
        => lineNumber == 0
            ? (Empty, this)
            : (this, Empty);

    internal override (Box<T> left, Box<T> right) CutVerticalCore(int columnNumber)
        => (
            new TextBox<T>(Line[..columnNumber]),
            new TextBox<T>(Line[columnNumber..])
        );

    internal override Box<T> OverlayCore(Box<T> background) => this;

    internal override async ValueTask WriteLine(IDocumentRenderer<T> renderer, int lineNumber, CancellationToken cancellationToken)
    {
        await renderer.Text(Line.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    internal override Box<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector)
        => new TextBox<U>(Line);
}

internal class AnnotatedBox<T> : Box<T>
{
    public T Value { get; }

    public Box<T> Box { get; }

    public AnnotatedBox(T value, Box<T> box)
        : base(box.Width, box.Height)
    {
        Value = value;
        Box = box;
    }

    internal override (Box<T> top, Box<T> bottom) CutHorizontalCore(int lineNumber)
    {
        var (top, bottom) = Box.CutHorizontal(lineNumber);
        return (top.Annotated(Value), bottom.Annotated(Value));
    }

    internal override (Box<T> left, Box<T> right) CutVerticalCore(int columnNumber)
    {
        var (left, right) = Box.CutVertical(columnNumber);
        return (left.Annotated(Value), right.Annotated(Value));
    }

    internal override Box<T> OverlayCore(Box<T> background)
        => Box.OverlayCore(background).Annotated(Value);

    internal override async ValueTask WriteLine(IDocumentRenderer<T> renderer, int lineNumber, CancellationToken cancellationToken)
    {
        await renderer.PushAnnotation(Value, cancellationToken).ConfigureAwait(false);
        await Box.WriteLine(renderer, lineNumber, cancellationToken).ConfigureAwait(false);
        await renderer.PopAnnotation(cancellationToken).ConfigureAwait(false);
    }

    internal override Box<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector)
        => selector(Value)
            .Aggregate(
                Box.MapAnnotationsCore(selector),
                (b, x) => b.Annotated(x)
            );
}

internal class VConcatBox<T> : Box<T>
{
    public Box<T> Top { get; }

    public Box<T> Bottom { get; }

    public VConcatBox(Box<T> top, Box<T> bottom)
        : base(AssertEqual(top.Width, bottom.Width), top.Height + bottom.Height)
    {
        Top = top;
        Bottom = bottom;
    }

    internal override (Box<T> top, Box<T> bottom) CutHorizontalCore(int lineNumber)
    {
        if (lineNumber <= Top.Height)
        {
            var (topTop, topBottom) = Top.CutHorizontal(lineNumber);
            return (topTop, topBottom.Above(Bottom));
        }

        var (bottomTop, bottomBottom) = Bottom.CutHorizontal(lineNumber - Top.Height);
        return (Top.Above(bottomTop), bottomBottom);
    }

    internal override (Box<T> left, Box<T> right) CutVerticalCore(int columnNumber)
    {
        var (topLeft, topRight) = Top.CutVertical(columnNumber);
        var (bottomLeft, bottomRight) = Bottom.CutVertical(columnNumber);
        return (topLeft.Above(bottomLeft), topRight.Above(bottomRight));
    }

    internal override Box<T> OverlayCore(Box<T> background)
    {
        var (backgroundTop, backgroundBottom) = background.CutHorizontal(Top.Height);
        return Top.OverlayCore(backgroundTop).Above(Bottom.OverlayCore(backgroundBottom));
    }

    internal override ValueTask WriteLine(IDocumentRenderer<T> renderer, int lineNumber, CancellationToken cancellationToken)
    {
        var (box, line) = lineNumber >= Top.Height
            ? (Bottom, lineNumber - Top.Height)
            : (Top, lineNumber);
        return box.WriteLine(renderer, line, cancellationToken);
    }

    internal override Box<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector)
        => new VConcatBox<U>(
            Top.MapAnnotationsCore(selector),
            Bottom.MapAnnotationsCore(selector)
        );
}

internal class HConcatBox<T> : Box<T>
{
    public Box<T> Left { get; }

    public Box<T> Right { get; }

    public HConcatBox(Box<T> left, Box<T> right)
        : base(left.Width + right.Width, AssertEqual(left.Height, right.Height))
    {
        Left = left;
        Right = right;
    }

    internal override (Box<T> top, Box<T> bottom) CutHorizontalCore(int lineNumber)
    {
        var (leftTop, leftBottom) = Left.CutHorizontal(lineNumber);
        var (rightTop, rightBottom) = Right.CutHorizontal(lineNumber);
        return (leftTop.LeftOf(rightTop), leftBottom.LeftOf(rightBottom));
    }

    internal override (Box<T> left, Box<T> right) CutVerticalCore(int columnNumber)
    {
        if (columnNumber <= Left.Width)
        {
            var (leftLeft, leftRight) = Left.CutVertical(columnNumber);
            return (leftLeft, leftRight.LeftOf(Right));
        }

        var (rightLeft, rightRight) = Right.CutVertical(columnNumber - Left.Width);
        return (Left.LeftOf(rightLeft), rightRight);
    }

    internal override Box<T> OverlayCore(Box<T> background)
    {
        var (backgroundLeft, backgroundRight) = background.CutVertical(Left.Width);
        return Left.OverlayCore(backgroundLeft).LeftOf(Right.OverlayCore(backgroundRight));
    }

    internal override async ValueTask WriteLine(IDocumentRenderer<T> renderer, int lineNumber, CancellationToken cancellationToken)
    {
        await Left.WriteLine(renderer, lineNumber, cancellationToken).ConfigureAwait(false);
        await Right.WriteLine(renderer, lineNumber, cancellationToken).ConfigureAwait(false);
    }

    internal override Box<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector)
        => new HConcatBox<U>(
            Left.MapAnnotationsCore(selector),
            Right.MapAnnotationsCore(selector)
        );
}
