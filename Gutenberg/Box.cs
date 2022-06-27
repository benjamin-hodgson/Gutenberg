using System.Diagnostics.CodeAnalysis;

using Gutenberg.Rendering;

namespace Gutenberg;

/// <summary>
/// Represents an immutable two-dimensional rectangular box of text.
/// The box can be rendered by an <see cref="IDocumentRenderer{T}"/>
/// or laid out as part of a <see cref="Document{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Boxes can be composed spatially to create bigger boxes:
/// they can be placed <see cref="LeftOf"/>, <see cref="RightOf"/>,
/// <see cref="Above"/>, and <see cref="Below"/> one another.
/// </para>
///
/// <para>
/// You can also put boxes <see cref="InFrontOf"/> and
/// <see cref="Behind"/> one another. Text contained in a box that's
/// placed in front of another box will obscure text contained in
/// in the background box. Additionally, boxes may contain
/// <see cref="Transparent"/> areas, through which background text
/// will be visible.
/// </para>
/// 
/// <para>
/// The box may contain annotations - values of type
/// <typeparamref name="T"/> - which can be interpreted by the
/// <see cref="IDocumentRenderer{T}"/>.
/// </para>
///
/// <para>
/// This class is intended to be imported under an alias,
/// since typically the type of annotations won't change within your code:
/// <c>using Box = Gutenberg.Box&lt;MyAnnotation&gt;;</c>
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The type of annotations in the box
/// </typeparam>
[SuppressMessage(
    "Design",
    "CA1000",  // "Do not declare static members on generic types"
    Justification = "This type is designed to be imported under an alias"
)]
public abstract class Box<T>
{
    /// <summary>The width of the <see cref="Box{T}"/> in characters.</summary>
    public int Width { get; }

    /// <summary>The width of the <see cref="Box{T}"/> in lines.</summary>
    public int Height { get; }

    private protected Box(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// An empty box, with zero <see cref="Width"/> and <see cref="Height"/>.
    /// </summary>
    public static Box<T> Empty { get; } = new EmptyBox<T>();

    /// <summary>
    /// <para>
    /// Creates a new <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="bottom"/> placed below it.
    /// </para>
    /// 
    /// <para>
    /// If either of the boxes is narrower than the other, it will be
    /// padded with a <see cref="Transparent"/> box until the
    /// <see cref="Width"/> matches, according to the chosen
    /// <paramref name="alignment"/>.
    /// </para>
    /// 
    /// <para>
    /// See the docs for <see cref="Alignment"/> for detailed 
    /// examples of how the <paramref name="alignment"/> affects
    /// the layout of the box.
    /// </para>
    /// </summary>
    /// <example name="Above example">
    /// <code doctest="true">
    /// var box = Box.FromString("abc")
    ///     .Above("def")
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌───┐
    /// // │abc│
    /// // │def│
    /// // └───┘
    /// </code>
    /// </example>
    /// <param name="bottom">
    /// The <see cref="Box{T}"/> to place below this one
    /// </param>
    /// <param name="alignment">
    /// How to align the boxes if one is narrower than the other.
    /// </param>
    /// <returns>
    /// A <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="bottom"/> placed below it.
    /// </returns>
    /// <seealso cref="Below"/>
    public Box<T> Above(Box<T> bottom, Alignment alignment = Alignment.Start)
    {
        ArgumentNullException.ThrowIfNull(bottom);
        return VConcat(this, bottom, alignment);
    }

    /// <summary>
    /// <para>
    /// Creates a new <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="top"/> placed above it.
    /// </para>
    /// 
    /// <para>
    /// If either of the boxes is narrower than the other, it will be
    /// padded with a <see cref="Transparent"/> box until the
    /// <see cref="Width"/> matches, according to the chosen
    /// <paramref name="alignment"/>.
    /// </para>
    /// 
    /// <para>
    /// See the docs for <see cref="Alignment"/> for detailed 
    /// examples of how the <paramref name="alignment"/> affects
    /// the layout of the box.
    /// </para>
    /// </summary>
    /// <example name="Below example">
    /// <code doctest="true">
    /// var box = Box.FromString("abc")
    ///     .Below("def")
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌───┐
    /// // │def│
    /// // │abc│
    /// // └───┘
    /// </code>
    /// </example>
    /// <param name="top">
    /// The <see cref="Box{T}"/> to place above this one
    /// </param>
    /// <param name="alignment">
    /// How to align the boxes if one is narrower than the other.
    /// </param>
    /// <returns>
    /// A <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="top"/> placed above it.
    /// </returns>
    /// <seealso cref="Above"/>
    public Box<T> Below(Box<T> top, Alignment alignment = Alignment.Start)
    {
        ArgumentNullException.ThrowIfNull(top);
        return VConcat(top, this, alignment);
    }

    /// <summary>
    /// <para>
    /// Creates a new <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="right"/> placed to its right.
    /// </para>
    /// 
    /// <para>
    /// If either of the boxes is shorter than the other, it will be
    /// padded with a <see cref="Transparent"/> box until the
    /// <see cref="Height"/> matches, according to the chosen
    /// <paramref name="alignment"/>.
    /// </para>
    /// 
    /// <para>
    /// See the docs for <see cref="Alignment"/> for detailed 
    /// examples of how the <paramref name="alignment"/> affects
    /// the layout of the box.
    /// </para>
    /// </summary>
    /// <example name="LeftOf example">
    /// <code doctest="true">
    /// var box = Box.FromString(string.Join('\n', "abc".AsEnumerable()))
    ///     .LeftOf(string.Join('\n', "def".AsEnumerable()))
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌──┐
    /// // │ad│
    /// // │be│
    /// // │cf│
    /// // └──┘
    /// </code>
    /// </example>
    /// <param name="right">
    /// The <see cref="Box{T}"/> to place to the right of this one
    /// </param>
    /// <param name="alignment">
    /// How to align the boxes if one is shorter than the other.
    /// </param>
    /// <returns>
    /// A <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="right"/> placed to the right of it.
    /// </returns>
    /// <seealso cref="RightOf"/>
    public Box<T> LeftOf(Box<T> right, Alignment alignment = Alignment.Start)
    {
        ArgumentNullException.ThrowIfNull(right);
        return HConcat(this, right, alignment);
    }

    /// <summary>
    /// <para>
    /// Creates a new <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="left"/> placed to its left.
    /// </para>
    /// 
    /// <para>
    /// If either of the boxes is shorter than the other, it will be
    /// padded with a <see cref="Transparent"/> box until the
    /// <see cref="Height"/> matches, according to the chosen
    /// <paramref name="alignment"/>.
    /// </para>
    /// 
    /// <para>
    /// See the docs for <see cref="Alignment"/> for detailed 
    /// examples of how the <paramref name="alignment"/> affects
    /// the layout of the box.
    /// </para>
    /// </summary>
    /// <example name="RightOf example">
    /// <code doctest="true">
    /// var box = Box.FromString(string.Join('\n', "abc".AsEnumerable()))
    ///     .RightOf(string.Join('\n', "def".AsEnumerable()))
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌──┐
    /// // │da│
    /// // │eb│
    /// // │fc│
    /// // └──┘
    /// </code>
    /// </example>
    /// <param name="left">
    /// The <see cref="Box{T}"/> to place to the left of this one
    /// </param>
    /// <param name="alignment">
    /// How to align the boxes if one is shorter than the other.
    /// </param>
    /// <returns>
    /// A <see cref="Box{T}"/> consisting of this
    /// box with <paramref name="left"/> placed to the left of it.
    /// </returns>
    /// <seealso cref="RightOf"/>
    public Box<T> RightOf(Box<T> left, Alignment alignment = Alignment.Start)
    {
        ArgumentNullException.ThrowIfNull(left);
        return HConcat(left, this, alignment);
    }

    /// <summary>
    /// <para>
    /// Creates a new <see cref="Box{T}"/> consisting of this
    /// box overlaid onto <paramref name="background"/>.
    /// </para>
    ///
    /// <para>
    /// Text contained within this box will obscure any corresponding
    /// text in the <paramref name="background"/>. If this box has
    /// <see cref="Transparent"/> areas, the <paramref name="background"/>
    /// will be visible through the transparent areas.
    /// </para>
    /// 
    /// <para>
    /// If either box is shorter or narrower than the other, it will be
    /// padded with a <see cref="Transparent"/> box until the sizes match,
    /// according to the chosen <paramref name="alignment"/>.
    /// </para>
    /// 
    /// <para>
    /// See the docs for <see cref="Alignment"/> for detailed 
    /// examples of how the <paramref name="alignment"/> affects
    /// the layout of the box.
    /// </para>
    /// </summary>
    /// <example name="InFrontOf example">
    /// <code doctest="true">
    /// // a box with a transparent window in the centre
    /// var mask = Box.Transparent(1, 1)
    ///     .LeftOf("*").RightOf("*")
    ///     .Above("***").Below("***");
    /// var box = mask
    ///     .InFrontOf("abcde\nfghij\nklmno", Alignment.CentreStart)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌─────┐
    /// // │a***e│
    /// // │f*h*j│
    /// // │k***o│
    /// // └─────┘
    /// </code>
    /// </example>
    /// <param name="background">
    /// The <see cref="Box{T}"/> to place behind this one
    /// </param>
    /// <param name="alignment">
    /// How to align the boxes if one is smaller than the other.
    /// </param>
    /// <returns>
    /// A <see cref="Box{T}"/> consisting of this
    /// box overlaid onto <paramref name="background"/>.
    /// </returns>
    /// <seealso cref="Behind"/>
    public Box<T> InFrontOf(Box<T> background, Alignment alignment = Alignment.Start)
    {
        ArgumentNullException.ThrowIfNull(background);
        return ZConcat(this, background, alignment);
    }

    /// <summary>
    /// <para>
    /// Returns a new <see cref="Box{T}"/> consisting of
    /// <paramref name="foreground"/> overlaid onto this one.
    /// </para>
    ///
    /// <para>
    /// Text contained within <paramref name="foreground"/> will obscure
    /// any corresponding text in this box. If <paramref name="foreground"/>
    /// has <see cref="Transparent"/> areas, the content of this box
    /// will be visible through the transparent areas.
    /// </para>
    /// 
    /// <para>
    /// If either box is shorter or narrower than the other, it will be
    /// padded with a <see cref="Transparent"/> box until the sizes match,
    /// according to the chosen <paramref name="alignment"/>.
    /// </para>
    /// 
    /// <para>
    /// See the docs for <see cref="Alignment"/> for detailed 
    /// examples of how the <paramref name="alignment"/> affects
    /// the layout of the box.
    /// </para>
    /// </summary>
    /// <example name="Behind example">
    /// <code doctest="true">
    /// // a box with a transparent window in the centre
    /// var mask = Box.Transparent(1, 1)
    ///     .LeftOf("*").RightOf("*")
    ///     .Above("***").Below("***");
    /// var box = Box.FromString("abcde\nfghij\nklmno")
    ///     .Behind(mask, Alignment.CentreStart)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌─────┐
    /// // │a***e│
    /// // │f*h*j│
    /// // │k***o│
    /// // └─────┘
    /// </code>
    /// </example>
    /// <param name="foreground">
    /// The <see cref="Box{T}"/> overlay onto this one
    /// </param>
    /// <param name="alignment">
    /// How to align the boxes if one is smaller than the other.
    /// </param>
    /// <returns>
    /// A <see cref="Box{T}"/> consisting of the content of
    /// <paramref name="foreground"/> overlaid onto this box.
    /// </returns>
    /// <seealso cref="InFrontOf"/>
    public Box<T> Behind(Box<T> foreground, Alignment alignment = Alignment.Start)
    {
        ArgumentNullException.ThrowIfNull(foreground);
        return ZConcat(foreground, this, alignment);
    }

    /// <summary>
    /// Apply an annotation to the current <see cref="Box{T}"/>.
    /// The annotation will be passed to the <see cref="IDocumentRenderer{T}"/>.
    /// </summary>
    /// <param name="value">The annotation</param>
    /// <returns>
    /// A copy of the current <see cref="Box{T}"/> with an annotation applied.
    /// </returns>
    public Box<T> Annotated(T value)
        => new AnnotatedBox<T>(value, this);

    /// <summary>
    /// Apply a function to all the annotations in the current document.
    /// </summary>
    /// <param name="selector">The function to apply to the annotations.</param>
    /// <returns>
    /// A document with all of the annotations replaced
    /// with the return value of <paramref name="selector"/>
    /// </returns>
    public Box<U> Select<U>(Func<T, U> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return MapAnnotations(selector);
    }

    /// <summary>
    /// Apply a function to all the annotations in the current document.
    /// </summary>
    /// <param name="selector">The function to apply to the annotations.</param>
    /// <returns>
    /// A document with all of the annotations replaced
    /// with the return value of <paramref name="selector"/>
    /// </returns>
    public Box<U> MapAnnotations<U>(Func<T, U> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return MapAnnotationsCore(x => new[] { selector(x) });
    }

    /// <summary>
    /// Apply a function to all the annotations in the current document.
    /// If the function returns multiple annotations,
    /// the annotations are added in a left-to-right fashion.
    /// </summary>
    /// <param name="selector">The function to apply to the annotations.</param>
    /// <returns>
    /// A document with all of the annotations replaced
    /// with the return values of <paramref name="selector"/>
    /// </returns>
    public Box<U> MapAnnotations<U>(Func<T, IEnumerable<U>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return MapAnnotationsCore(selector);
    }

    /// <summary>Render this <see cref="Box{T}"/> as a string</summary>
    [SuppressMessage(
        "design",
        "CA1065",
        Justification = "The exceptions are just asserts, they should never be raised"
    )]  // "Exceptions should not be raised in this type of method."
    public override string ToString()
    {
        using var writer = new StringWriter();
        var task = Render((IDocumentRenderer<T>)new PlainTextDocumentRenderer(writer));

        // neither of these should ever happen
        if (!task.IsCompleted)
        {
            throw new InvalidOperationException("ToString went async! Please report this as a bug in Gutenberg");
        }
        if (task.IsFaulted)
        {
            throw new InvalidOperationException(
                "ToString threw! Please report this as a bug in Gutenberg.",
                task.AsTask().Exception
            );
        }

        return writer.ToString();
    }

    /// <summary>
    /// Write the contents of this <see cref="Box{T}"/> into
    /// the <paramref name="renderer"/>.
    /// </summary>
    /// <param name="renderer">The renderer</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>
    /// A <see cref="ValueTask"/> which completes when
    /// the box has been written to the <paramref name="renderer"/>.
    /// </returns>
    public async ValueTask Render(IDocumentRenderer<T> renderer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        for (var i = 0; i < Height; i++)
        {
            await WriteLine(renderer, i, cancellationToken).ConfigureAwait(false);
            await renderer.NewLine(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creates a transparent <see cref="Box{T}"/> with the
    /// specified dimensions. If the box is placed <see cref="InFrontOf"/>
    /// another box, the other box will be visible through the
    /// transparent box.
    /// </summary>
    /// <param name="width">The width of the box</param>
    /// <param name="height">The height of the box</param>
    /// <returns>A transparent <see cref="Box{T}"/></returns>
    public static Box<T> Transparent(int width, int height)
    {
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width cannot be negative");
        }
        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height cannot be negative");
        }
        return new TransparentBox<T>(width, height);
    }

    /// <summary>
    /// Creates a <see cref="Box{T}"/> containing the
    /// specified <paramref name="text"/>.
    /// </summary>
    /// <remarks>
    /// If the text contains line breaks, the resulting box's height
    /// will be equal to the number of lines in the text,
    /// and its width will be equal to the length of the longest line.
    /// Each line will be left-aligned.
    /// </remarks>
    /// <param name="text">
    /// The text to place into a <see cref="Box{T}"/>
    /// </param>
    /// <returns>
    /// A <see cref="Box{T}"/> containing the
    /// specified <paramref name="text"/>.
    /// </returns>
    public static Box<T> FromString(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text
            .SplitLines()
            .Select(line => new TextBox<T>(line))
            .Aggregate(Empty, (z, x) => z.Above(x));
    }

    /// <summary>
    /// Creates a <see cref="Box{T}"/> consisting of the current
    /// box with a border drawn around it.
    /// </summary>
    /// <example name="WithBorder example">
    /// <code doctest="true">
    /// var box = Box.FromString("abc\ndef\nghi")
    ///     .WithBorder();
    /// Console.Write(box);
    /// // Output:
    /// // ┌───┐
    /// // │abc│
    /// // │def│
    /// // │ghi│
    /// // └───┘
    /// </code>
    /// </example>
    /// <returns>
    /// A <see cref="Box{T}"/> consisting of the current
    /// box with a border drawn around it.
    /// </returns>
    public Box<T> WithBorder()
    {
        var horiz = FromString(new string('─', Width));
        var vert = FromString(string.Join('\n', Enumerable.Repeat('│', Height)));
        var left = vert.Above("└").Below("┌");
        var right = vert.Above("┘").Below("┐");
        return Above(horiz)
            .Below(horiz)
            .RightOf(left)
            .LeftOf(right);
    }

    /// <summary>
    /// Implicitly convert a string to a <see cref="Box{T}"/>
    /// </summary>
    /// <remarks>
    /// This implicit conversion is equivalent to <see cref="FromString"/>.
    /// </remarks>
    public static implicit operator Box<T>(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        return FromString(str);
    }

    private static Box<T> VConcat(Box<T> top, Box<T> bottom, Alignment alignment)
    {
        if (top.Height == 0)
        {
            return bottom;
        }
        if (bottom.Height == 0)
        {
            return top;
        }
        return new VConcatBox<T>(
            top.PadWidthToAtLeast(bottom.Width, alignment),
            bottom.PadWidthToAtLeast(top.Width, alignment)
        );
    }

    private static Box<T> HConcat(Box<T> left, Box<T> right, Alignment alignment)
    {
        if (left.Width == 0)
        {
            return right;
        }
        if (right.Width == 0)
        {
            return left;
        }
        return new HConcatBox<T>(
            left.PadHeightToAtLeast(right.Height, alignment),
            right.PadHeightToAtLeast(left.Height, alignment)
        );
    }

    private static Box<T> ZConcat(Box<T> foreground, Box<T> background, Alignment alignment)
    {
        if (foreground.Width >= background.Width && foreground.Height >= background.Height)
        {
            return foreground;
        }
        if (foreground.Width == 0 || foreground.Height == 0)
        {
            return background;
        }
        var f = foreground
            .PadHeightToAtLeast(background.Height, alignment)
            .PadWidthToAtLeast(background.Width, alignment);
        var b = background
            .PadHeightToAtLeast(foreground.Height, alignment)
            .PadWidthToAtLeast(foreground.Width, alignment);
        return f.OverlayCore(b);
    }

    private Box<T> PadHeightToAtLeast(int targetHeight, Alignment alignment)
    {
        if (Height >= targetHeight)
        {
            return this;
        }
        var delta = targetHeight - Height;
        var smaller = delta / 2;
        var larger = delta - smaller;
        return alignment switch
        {
            Alignment.Start => new VConcatBox<T>(
                this,
                new TransparentBox<T>(Width, delta)
            ),
            Alignment.CentreStart => new VConcatBox<T>(
                new VConcatBox<T>(
                    new TransparentBox<T>(Width, smaller),
                    this
                ),
                new TransparentBox<T>(Width, larger)
            ),
            Alignment.CentreEnd => new VConcatBox<T>(
                new VConcatBox<T>(
                    new TransparentBox<T>(Width, larger),
                    this
                ),
                new TransparentBox<T>(Width, smaller)
            ),
            Alignment.End => new VConcatBox<T>(
                new TransparentBox<T>(Width, delta),
                this
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "Unknown alignment"),
        };
    }

    private Box<T> PadWidthToAtLeast(int targetWidth, Alignment alignment)
    {
        if (Width >= targetWidth)
        {
            return this;
        }
        var delta = targetWidth - Width;
        var smaller = delta / 2;
        var larger = delta - smaller;
        return alignment switch
        {
            Alignment.Start => new HConcatBox<T>(
                this,
                new TransparentBox<T>(delta, Height)
            ),
            Alignment.CentreStart => new HConcatBox<T>(
                new HConcatBox<T>(
                    new TransparentBox<T>(smaller, Height),
                    this
                ),
                new TransparentBox<T>(larger, Height)
            ),
            Alignment.CentreEnd => new HConcatBox<T>(
                new HConcatBox<T>(
                    new TransparentBox<T>(larger, Height),
                    this
                ),
                new TransparentBox<T>(smaller, Height)
            ),
            Alignment.End => new HConcatBox<T>(
                new TransparentBox<T>(delta, Height),
                this
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "Unknown alignment"),
        };
    }

    internal (Box<T> top, Box<T> bottom) CutHorizontal(int lineNumber)
    {
        if (lineNumber < 0 || lineNumber > Height)
        {
            throw new ArgumentOutOfRangeException(nameof(lineNumber), lineNumber, "Line number out of range. Please report this as a bug in Gutenberg");
        }
        if (lineNumber == 0)
        {
            return (Empty, this);
        }
        if (lineNumber == Height)
        {
            return (this, Empty);
        }
        return CutHorizontalCore(lineNumber);
    }

    internal (Box<T> left, Box<T> right) CutVertical(int columnNumber)
    {
        if (columnNumber < 0 || columnNumber > Width)
        {
            throw new ArgumentOutOfRangeException(nameof(columnNumber), columnNumber, "Column number out of range. Please report this as a bug in Gutenberg");
        }
        if (columnNumber == 0)
        {
            return (Empty, this);
        }
        if (columnNumber == Width)
        {
            return (this, Empty);
        }
        return CutVerticalCore(columnNumber);
    }

    internal abstract (Box<T> top, Box<T> bottom) CutHorizontalCore(int lineNumber);
    internal abstract (Box<T> left, Box<T> right) CutVerticalCore(int columnNumber);
    internal abstract Box<T> OverlayCore(Box<T> background);
    internal abstract ValueTask WriteLine(IDocumentRenderer<T> renderer, int lineNumber, CancellationToken cancellationToken);
    internal abstract Box<U> MapAnnotationsCore<U>(Func<T, IEnumerable<U>> selector);

    private protected static int AssertEqual(int x, int y)
    {
        if (x != y)
        {
            throw new InvalidOperationException("Tried to compose two boxes with mismatched sizes. Please report this as a bug in Gutenberg");
        }
        return x;
    }
}
