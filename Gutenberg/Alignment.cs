namespace Gutenberg;

/// <summary>
/// How to behave when composing two <see cref="Box{T}"/>es
/// with mismatched sizes.
/// </summary>
public enum Alignment
{
    /// <summary>
    /// Align the smaller box with the start (the top or left)
    /// of the larger one.
    /// </summary>
    /// <example name="Alignment.Start - vertical">
    /// When boxes are being composed vertically,
    /// the start of the alignment axis is the
    /// left-hand side of the box.
    /// <code doctest="true">
    /// var box = Box.FromString("abcdefgh")
    ///     .Above("ijk", Alignment.Start)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌────────┐
    /// // │abcdefgh│
    /// // │ijk     │
    /// // └────────┘
    /// </code>
    /// </example>
    /// <example name="Alignment.Start - horizontal">
    /// When boxes are being composed horiztonally,
    /// the start of the alignment axis is the
    /// top of the box.
    /// <code doctest="true">
    /// var box = Box.FromString(string.Join('\n', "abcdefgh".AsEnumerable()))
    ///     .LeftOf(string.Join('\n', "ijk".AsEnumerable()), Alignment.Start)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌──┐
    /// // │ai│
    /// // │bj│
    /// // │ck│
    /// // │d │
    /// // │e │
    /// // │f │
    /// // │g │
    /// // │h │
    /// // └──┘
    /// </code>
    /// </example>
    Start,

    /// <summary>
    /// Align the smaller box with the centre of the larger one.
    /// If the box can't be exactly centred (because the width
    /// difference is an odd number), align it one character closer
    /// to the start.
    /// </summary>
    /// <example name="Alignment.CenterStart - vertical">
    /// When boxes are being composed vertically,
    /// the start of the alignment axis is the
    /// left-hand side of the box.
    /// <code doctest="true">
    /// var box = Box.FromString("abcdefgh")
    ///     .Above("ijk", Alignment.CenterStart)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌────────┐
    /// // │abcdefgh│
    /// // │  ijk   │
    /// // └────────┘
    /// </code>
    /// </example>
    /// <example name="Alignment.CenterStart - horizontal">
    /// When boxes are being composed horiztonally,
    /// the start of the alignment axis is the
    /// top of the box.
    /// <code doctest="true">
    /// var box = Box.FromString(string.Join('\n', "abcdefgh".AsEnumerable()))
    ///     .LeftOf(string.Join('\n', "ijk".AsEnumerable()), Alignment.CenterStart)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌──┐
    /// // │a │
    /// // │b │
    /// // │ci│
    /// // │dj│
    /// // │ek│
    /// // │f │
    /// // │g │
    /// // │h │
    /// // └──┘
    /// </code>
    /// </example>
    CenterStart,

    /// <summary>
    /// Align the smaller box with the centre of the larger one.
    /// If the box can't be exactly centred (because the width
    /// difference is an odd number), align it one character closer
    /// to the end.
    /// </summary>
    /// <example name="Alignment.CenterEnd - vertical">
    /// When boxes are being composed vertically,
    /// the start of the alignment axis is the
    /// left-hand side of the box.
    /// <code doctest="true">
    /// var box = Box.FromString("abcdefgh")
    ///     .Above("ijk", Alignment.CenterEnd)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌────────┐
    /// // │abcdefgh│
    /// // │   ijk  │
    /// // └────────┘
    /// </code>
    /// </example>
    /// <example name="Alignment.CenterEnd - horizontal">
    /// When boxes are being composed horiztonally,
    /// the start of the alignment axis is the
    /// top of the box.
    /// <code doctest="true">
    /// var box = Box.FromString(string.Join('\n', "abcdefgh".AsEnumerable()))
    ///     .LeftOf(string.Join('\n', "ijk".AsEnumerable()), Alignment.CenterEnd)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌──┐
    /// // │a │
    /// // │b │
    /// // │c │
    /// // │di│
    /// // │ej│
    /// // │fk│
    /// // │g │
    /// // │h │
    /// // └──┘
    /// </code>
    /// </example>
    CenterEnd,

    /// <summary>
    /// Align the smaller box with the end (the bottom or right)
    /// of the larger one.
    /// </summary>
    /// <example name="Alignment.End - vertical">
    /// When boxes are being composed vertically,
    /// the start of the alignment axis is the
    /// left-hand side of the box.
    /// <code doctest="true">
    /// var box = Box.FromString("abcdefgh")
    ///     .Above("ijk", Alignment.End)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌────────┐
    /// // │abcdefgh│
    /// // │     ijk│
    /// // └────────┘
    /// </code>
    /// </example>
    /// <example name="Alignment.End - horizontal">
    /// When boxes are being composed horiztonally,
    /// the start of the alignment axis is the
    /// top of the box.
    /// <code doctest="true">
    /// var box = Box.FromString(string.Join('\n', "abcdefgh".AsEnumerable()))
    ///     .LeftOf(string.Join('\n', "ijk".AsEnumerable()), Alignment.End)
    ///     .WithBorder();
    /// Console.Write(box.ToString());
    /// // Output:
    /// // ┌──┐
    /// // │a │
    /// // │b │
    /// // │c │
    /// // │d │
    /// // │e │
    /// // │fi│
    /// // │gj│
    /// // │hk│
    /// // └──┘
    /// </code>
    /// </example>
    End
}
