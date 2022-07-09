using System.Collections.Immutable;

using Gutenberg.Brackets;

namespace Gutenberg.Expression;

/// <summary>
/// The associativity of the  <see cref="BinaryOperator{T}"/>:
/// left-associative, right-associative, or non-associative
/// </summary>
public enum BinaryOperatorType
{
    /// <summary>
    /// Denotes a non-associative binary operator
    /// </summary>
    NonAssociative,
    /// <summary>
    /// Denotes a left-associative binary operator
    /// </summary>
    LeftAssociative,
    /// <summary>
    /// Denotes a right-associative binary operator
    /// </summary>
    RightAssociative
}

/// <summary>
/// Represents a binary (infix) operator with a given precedence
/// </summary>
/// <typeparam name="T">
/// The type of annotations in the docuemnt
/// </typeparam>
/// <seealso cref="OperatorFactory{T}"/>
/// <seealso cref="UnaryOperator{T}"/>
public sealed class BinaryOperator<T>
{
    private readonly BinaryOperatorType _type;
    private readonly int _precedence;
    private readonly Document<T> _symbol;
    private readonly IBracketer<T> _bracketer;

    internal BinaryOperator(
        BinaryOperatorType type,
        int precedence,
        Document<T> symbol,
        IBracketer<T> bracketer
    )
    {
        _type = type;
        _precedence = precedence;
        _symbol = symbol;
        _bracketer = bracketer;
    }

    /// <summary>
    /// Creates an <see cref="Expression{T}"/> representing
    /// this operator placed between the <paramref name="left"/>
    /// and <paramref name="right"/> expressions.
    /// </summary>
    /// <param name="left">
    /// The left-hand argument of the operator
    /// </param>
    /// <param name="right">
    /// The right-hand argument of the operator
    /// </param>
    /// <returns>
    /// An <see cref="Expression{T}"/> representing
    /// this operator placed between the <paramref name="left"/>
    /// and <paramref name="right"/> expressions.
    /// </returns>
    public Expression<T> Apply(Expression<T> left, Expression<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return new OperatorExpression<T>(
            _precedence,
            ImmutableArray.CreateRange(
                new[]
                {
                    left.BumpedIf(_type != BinaryOperatorType.LeftAssociative),
                    _symbol,
                    right.BumpedIf(_type != BinaryOperatorType.RightAssociative)
                }
            ),
            _bracketer
        );
    }
}

