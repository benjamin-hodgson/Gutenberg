using System.Diagnostics.CodeAnalysis;

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
    /// <summary>
    /// Creates a <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="type"/> and
    /// <paramref name="precedence"/>.
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
    /// <returns>
    /// A <see cref="UnaryOperator{T}"/>.
    /// </returns>
    public static UnaryOperator<T> Unary(
        UnaryOperatorType type,
        int precedence,
        Document<T> symbol
    )
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(UnaryOperatorType)}");
        }
        ArgumentNullException.ThrowIfNull(symbol);
        return new(type, precedence, symbol);
    }

    /// <summary>
    /// Creates a prefix <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <returns>
    /// A prefix <see cref="UnaryOperator{T}"/>.
    /// </returns>
    public static UnaryOperator<T> Prefix(int precedence, Document<T> symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        return Unary(UnaryOperatorType.Prefix, precedence, symbol);
    }
    
    /// <summary>
    /// Creates a postfix <see cref="UnaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
    /// </summary>
    /// <param name="precedence">
    /// The precedence of the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <param name="symbol">
    /// How to display the <see cref="UnaryOperator{T}"/>
    /// </param>
    /// <returns>
    /// A postfix <see cref="UnaryOperator{T}"/>.
    /// </returns>
    public static UnaryOperator<T> Postfix(int precedence, Document<T> symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        return Unary(UnaryOperatorType.Postfix, precedence, symbol);
    }

    /// <summary>
    /// Creates a <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="type"/> and
    /// <paramref name="precedence"/>.
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
    public static BinaryOperator<T> Infix(
        BinaryOperatorType type,
        int precedence,
        Document<T> symbol
    )
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(BinaryOperatorType)}");
        }
        ArgumentNullException.ThrowIfNull(symbol);
        return new(BinaryOperatorType.NonAssociative, precedence, symbol);
    }

    /// <summary>
    /// Creates a non-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
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
    public static BinaryOperator<T> InfixN(int precedence, Document<T> symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        return new(BinaryOperatorType.NonAssociative, precedence, symbol);
    }
    
    /// <summary>
    /// Creates a left-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
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
    public static BinaryOperator<T> InfixL(int precedence, Document<T> symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        return new(BinaryOperatorType.LeftAssociative, precedence, symbol);
    }

    /// <summary>
    /// Creates a right-associative <see cref="BinaryOperator{T}"/>
    /// representing the given <paramref name="symbol"/>,
    /// with the given <paramref name="precedence"/>.
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
    public static BinaryOperator<T> InfixR(int precedence, Document<T> symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        return new(BinaryOperatorType.RightAssociative, precedence, symbol);
    }
}
