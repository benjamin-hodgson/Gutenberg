using System.Collections.Immutable;

namespace Gutenberg.Expression;

/// <summary>
/// The type of the <see cref="UnaryOperator{T}"/>:
/// prefix or postfix
/// </summary>
public enum UnaryOperatorType
{
    /// <summary>
    /// Denotes a prefix operator
    /// </summary>
    Prefix,
    /// <summary>
    /// Denotes a postfix operator
    /// </summary>
    Postfix
}

/// <summary>
/// Represents a unary (prefix or postfix)
/// operator with a given precedence
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the docuemnt
/// </typeparam>
/// <seealso cref="OperatorFactory{T}"/>
/// <seealso cref="BinaryOperator{T}"/>
public sealed class UnaryOperator<T>
{
    private readonly UnaryOperatorType _type;
    private readonly int _precedence;
    private readonly Document<T> _symbol;

    internal UnaryOperator(
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

        _type = type;
        _precedence = precedence;
        _symbol = symbol;
    }

    /// <summary>
    /// Creates an <see cref="Expression{T}"/> representing
    /// this operator applied to the <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">
    /// The expression to apply this operator to
    /// </param>
    /// <returns>
    /// An <see cref="Expression{T}"/> representing
    /// this operator applied to the <paramref name="expression"/>.
    /// </returns>
    public Expression<T> Apply(Expression<T> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return new OperatorExpression<T>(
            _precedence,
            _type switch
            {
                UnaryOperatorType.Prefix
                    => ImmutableArray.CreateRange(new[] { _symbol, expression }),
                UnaryOperatorType.Postfix
                    => ImmutableArray.CreateRange(new[] { expression, _symbol }),
                _ => throw new InvalidOperationException($"Unknown {nameof(UnaryOperatorType)}"),
            }
        );
    }
}

