using System.Diagnostics.CodeAnalysis;

using Gutenberg.Brackets;

namespace Gutenberg.Expression;

/// <summary>
/// Methods to create <see cref="UnaryOperator{T}"/> and
/// <see cref="BinaryOperator{T}"/> values.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1000",  // "Do not declare static members on generic types"
    Justification = "This type is designed to be imported under an alias"
)]
public static class OperatorFactory<T>
{
    private static readonly IBracketer<T> _defaultBracketer
        = new KernighanRitchieBracketer<T>("(", ")");

    /// <summary>
    /// Creates a <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="type"/> and
    /// <paramref name="precedence"/>. When the expression
    /// needs to be surrounded by parentheses, the K&amp;R
    /// style is used.
    /// </summary>
    /// <param name="type">
    /// The type of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="chainable">
    /// Whether the operator is chainable. If a non-chainable
    /// operator is applied multiple times, the inner expressions
    /// are parenthesised.
    /// </param>
    /// <returns>
    /// A <see cref="UnaryOperator{T}"/>.
    /// </returns>
    /// <seealso cref="KernighanRitchieBracketer{T}"/>
    public static UnaryOperator<T> Unary(
        UnaryOperatorType type,
        int precedence,
        Document<T> symbol,
        bool chainable = false
    ) => Unary(
        type,
        precedence,
        symbol,
        chainable,
        _defaultBracketer
    );

    /// <summary>
    /// Creates a <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="type"/> and
    /// <paramref name="precedence"/>.When the expression
    /// needs to be surrounded by parentheses,
    /// <paramref name="bracketer"/> is used.
    /// </summary>
    /// <param name="type">
    /// The type of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="chainable">
    /// Whether the operator is chainable. If a non-chainable
    /// operator is applied multiple times, the inner expressions
    /// are parenthesised.
    /// </param>
    /// <param name="bracketer">
    /// An <see cref="IBracketer{T}"/> which will be called when
    /// the operator needs to be parenthesised.
    /// </param>
    /// <returns>
    /// A <see cref="UnaryOperator{T}"/>.
    /// </returns>
    public static UnaryOperator<T> Unary(
        UnaryOperatorType type,
        int precedence,
        Document<T> symbol,
        bool chainable,
        IBracketer<T> bracketer
    )
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(UnaryOperatorType)}");
        }
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(bracketer);

        return new(type, precedence, symbol, chainable, bracketer);
    }

    /// <summary>
    /// Creates a prefix <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// the K&amp;R style is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="chainable">
    /// Whether the operator is chainable. If a non-chainable
    /// operator is applied multiple times, the inner expressions
    /// are parenthesised.
    /// </param>
    /// <returns>
    /// A prefix <see cref="UnaryOperator{T}"/>.
    /// </returns>
    /// <seealso cref="KernighanRitchieBracketer{T}"/>
    public static UnaryOperator<T> Prefix(
        int precedence,
        Document<T> symbol,
        bool chainable = false
    ) => Prefix(precedence, symbol, chainable, _defaultBracketer);

    /// <summary>
    /// Creates a prefix <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// <paramref name="bracketer"/> is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="chainable">
    /// Whether the operator is chainable. If a non-chainable
    /// operator is applied multiple times, the inner expressions
    /// are parenthesised.
    /// </param>
    /// <param name="bracketer">
    /// An <see cref="IBracketer{T}"/> which will be called when
    /// the operator needs to be parenthesised.
    /// </param>
    /// <returns>
    /// A prefix <see cref="UnaryOperator{T}"/>.
    /// </returns>
    public static UnaryOperator<T> Prefix(
        int precedence,
        Document<T> symbol,
        bool chainable,
        IBracketer<T> bracketer
    )
    {
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(bracketer);

        return Unary(UnaryOperatorType.Prefix, precedence, symbol, chainable, bracketer);
    }

    /// <summary>
    /// Creates a postfix <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// the K&amp;R style is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="chainable">
    /// Whether the operator is chainable. If a non-chainable
    /// operator is applied multiple times, the inner expressions
    /// are parenthesised.
    /// </param>
    /// <returns>
    /// A postfix <see cref="UnaryOperator{T}"/>.
    /// </returns>
    /// <seealso cref="KernighanRitchieBracketer{T}"/>
    public static UnaryOperator<T> Postfix(
        int precedence,
        Document<T> symbol,
        bool chainable = false
    ) => Postfix(precedence, symbol, chainable, _defaultBracketer);

    /// <summary>
    /// Creates a postfix <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// <paramref name="bracketer"/> is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="chainable">
    /// Whether the operator is chainable. If a non-chainable
    /// operator is applied multiple times, the inner expressions
    /// are parenthesised.
    /// </param>
    /// <param name="bracketer">
    /// An <see cref="IBracketer{T}"/> which will be called when
    /// the operator needs to be parenthesised.
    /// </param>
    /// <returns>
    /// A postfix <see cref="UnaryOperator{T}"/>.
    /// </returns>
    public static UnaryOperator<T> Postfix(
        int precedence,
        Document<T> symbol,
        bool chainable,
        IBracketer<T> bracketer
    )
    {
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(bracketer);

        return Unary(UnaryOperatorType.Postfix, precedence, symbol, chainable, bracketer);
    }

    /// <summary>
    /// Creates a <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="type"/> and
    /// <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// the K&amp;R style is used.
    /// </summary>
    /// <param name="type">
    /// The type of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <returns>
    /// A <see cref="BinaryOperator{T}"/>.
    /// </returns>
    /// <seealso cref="KernighanRitchieBracketer{T}"/>
    public static BinaryOperator<T> Infix(
        BinaryOperatorType type,
        int precedence,
        Document<T> symbol
    ) => Infix(
        type,
        precedence,
        symbol,
        _defaultBracketer
    );

    /// <summary>
    /// Creates a <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="type"/> and
    /// <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// <paramref name="bracketer"/> is used.
    /// </summary>
    /// <param name="type">
    /// The type of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="bracketer">
    /// An <see cref="IBracketer{T}"/> which will be called when
    /// the operator needs to be parenthesised.
    /// </param>
    /// <returns>
    /// A <see cref="BinaryOperator{T}"/>.
    /// </returns>
    public static BinaryOperator<T> Infix(
        BinaryOperatorType type,
        int precedence,
        Document<T> symbol,
        IBracketer<T> bracketer
    )
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(BinaryOperatorType)}");
        }
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(bracketer);

        return new(type, precedence, symbol, bracketer);
    }

    /// <summary>
    /// Creates a non-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// the K&amp;R style is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <returns>
    /// A non-associative <see cref="BinaryOperator{T}"/>.
    /// </returns>
    /// <seealso cref="KernighanRitchieBracketer{T}"/>
    public static BinaryOperator<T> InfixN(int precedence, Document<T> symbol)
        => InfixN(
            precedence,
            symbol,
            _defaultBracketer
        );

    /// <summary>
    /// Creates a non-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// <paramref name="bracketer"/> is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="bracketer">
    /// An <see cref="IBracketer{T}"/> which will be called when
    /// the operator needs to be parenthesised.
    /// </param>
    /// <returns>
    /// A non-associative <see cref="BinaryOperator{T}"/>.
    /// </returns>
    public static BinaryOperator<T> InfixN(int precedence, Document<T> symbol, IBracketer<T> bracketer)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(bracketer);

        return Infix(BinaryOperatorType.NonAssociative, precedence, symbol, bracketer);
    }

    /// <summary>
    /// Creates a left-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// the K&amp;R style is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <returns>
    /// A left-associative <see cref="BinaryOperator{T}"/>.
    /// </returns>
    /// <seealso cref="KernighanRitchieBracketer{T}"/>
    public static BinaryOperator<T> InfixL(int precedence, Document<T> symbol)
        => InfixL(precedence, symbol, _defaultBracketer);

    /// <summary>
    /// Creates a left-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// <paramref name="bracketer"/> is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="bracketer">
    /// An <see cref="IBracketer{T}"/> which will be called when
    /// the operator needs to be parenthesised.
    /// </param>
    /// <returns>
    /// A left-associative <see cref="BinaryOperator{T}"/>.
    /// </returns>
    public static BinaryOperator<T> InfixL(int precedence, Document<T> symbol, IBracketer<T> bracketer)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(bracketer);

        return Infix(BinaryOperatorType.LeftAssociative, precedence, symbol, bracketer);
    }

    /// <summary>
    /// Creates a right-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// the K&amp;R style is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <returns>
    /// A right-associative <see cref="BinaryOperator{T}"/>.
    /// </returns>
    /// <seealso cref="KernighanRitchieBracketer{T}"/>
    public static BinaryOperator<T> InfixR(int precedence, Document<T> symbol)
        => InfixR(precedence, symbol, _defaultBracketer);

    /// <summary>
    /// Creates a right-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// When the expression needs to be surrounded by parentheses,
    /// <paramref name="bracketer"/> is used.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="BinaryOperator{T}"/>
    /// </param>
    /// <param name="bracketer">
    /// An <see cref="IBracketer{T}"/> which will be called when
    /// the operator needs to be parenthesised.
    /// </param>
    /// <returns>
    /// A right-associative <see cref="BinaryOperator{T}"/>.
    /// </returns>
    public static BinaryOperator<T> InfixR(int precedence, Document<T> symbol, IBracketer<T> bracketer)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        return Infix(BinaryOperatorType.RightAssociative, precedence, symbol, bracketer);
    }
}
