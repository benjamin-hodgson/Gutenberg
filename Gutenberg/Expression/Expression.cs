using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Gutenberg.Bracketing;

namespace Gutenberg.Expression;

/// <summary>
/// <para>
/// Represents an expression composed of operators
/// applied to <see cref="Document{T}"/>s.
/// </para>
/// <para>
/// You can use <see cref="Expression{T}"/> to pretty-print
/// code in an expression-based language with operators and
/// precedence. <see cref="Expression{T}"/> will
/// automatically insert parentheses into the resulting document
/// to create an unambiguous display.
/// </para>
/// <para>
/// To get started, use the <see cref="OperatorFactory{T}"/> 
/// to create objects representing each of the operators in
/// your language. Then those operators can be applied to 
/// <see cref="Document{T}"/>s and other expressions to create
/// complex expressions. Finally, when you <see cref="PrettyPrint"/>
/// the expression, it will return a <see cref="Document{T}"/> with
/// parentheses inserted in the correct locations.
/// </para>
/// </summary>
/// <example name="ExpressionExample">
/// Here is an example of a small expression language with
/// negation, addition, and equality operators.
/// Since <c>!</c> binds tighter (has a higher precedence) than
/// <c>==</c>, the example document pretty-prints itself
/// with parentheses around the equality expression.
/// <code doctest="true">
/// var not = OperatorFactory&lt;object&gt;.Prefix(7, "!");
/// var plus = OperatorFactory&lt;object&gt;.InfixL(5, " + ");
/// var eq = OperatorFactory&lt;object&gt;.InfixL(3, " == ");
///
/// var doc = not.Apply(eq.Apply(plus.Apply("x", "y"), "z"));
/// Console.WriteLine(doc.PrettyPrint());
/// // Output:
/// // !(x + y == z)
/// </code>
/// </example>
/// <typeparam name="T">
/// The type of annotations in the <see cref="Document{T}"/>s.
/// </typeparam>
[SuppressMessage(
    "Design",
    "CA1000",  // "Do not declare static members on generic types"
    Justification = "This type is designed to be imported under an alias"
)]
[SuppressMessage("Design", "CA1724")]  // "The type name conflicts in whole or in part with the namespace name"
public abstract class Expression<T> : IPrettyPrintable<T>
{
    /// <inheritdoc cref="IPrettyPrintable{T}.PrettyPrint" />
    public Document<T> PrettyPrint() => ToDocument(PrecedenceState.Default);

    internal abstract Document<T> ToDocument(PrecedenceState state);

    internal Expression<T> Bumped()
        => new BumpedExpression<T>(this);
    internal Expression<T> BumpedIf(bool condition)
        => condition ? Bumped() : this;

    /// <summary>
    /// Creates a leaf-level <see cref="Expression{T}"/>
    /// from the given string.
    /// </summary>
    /// <remarks>
    /// <c>Expr.FromString(x)</c> is equivalent to
    /// <c>Expr.FromDocument(Doc.FromString(x))</c>.
    /// </remarks>
    /// <param name="text">The document</param>
    /// <returns>
    /// An expression representing the <paramref name="text"/>
    /// </returns>
    /// <seealso cref="FromDocument"/>
    public static Expression<T> FromString(string text)
        => FromDocument(Document<T>.FromString(text));

    /// <summary>
    /// Creates a leaf-level <see cref="Expression{T}"/>
    /// from the given <paramref name="document"/>.
    /// </summary>
    /// <param name="document">The document</param>
    /// <returns>
    /// An expression representing the <paramref name="document"/>
    /// </returns>
    public static Expression<T> FromDocument(Document<T> document)
        => new DocumentExpression<T>(document);

    /// <summary>
    /// Implicitly converts a <see cref="Document{T}"/>
    /// to an <see cref="Expression{T}"/>.
    /// </summary>
    /// <remarks>
    /// This conversion is equivalent to <see cref="FromDocument"/>.
    /// </remarks>
    /// <param name="document">The document</param>
    /// <returns>
    /// An expression representing the <paramref name="document"/>
    /// </returns>
    /// <seealso cref="FromDocument"/>
    public static implicit operator Expression<T>(Document<T> document)
        => FromDocument(document);

    /// <summary>
    /// Implicitly converts a string
    /// to an <see cref="Expression{T}"/>.
    /// </summary>
    /// <remarks>
    /// This conversion is equivalent to <see cref="FromString"/>.
    /// </remarks>
    /// <param name="text">The text</param>
    /// <returns>
    /// An expression representing the <paramref name="text"/>
    /// </returns>
    /// <seealso cref="FromString"/>
    public static implicit operator Expression<T>(string text)
        => FromString(text);
}

internal class OperatorExpression<T> : Expression<T>
{
    private readonly int _precedence;
    private readonly ImmutableArray<Expression<T>> _expressions;
    private readonly IBracketer<T> _bracketer;

    internal OperatorExpression(
        int precedence,
        ImmutableArray<Expression<T>> expressions,
        IBracketer<T> bracketer
    )
    {
        _precedence = precedence;
        _expressions = expressions;
        _bracketer = bracketer;
    }

    internal override Document<T> ToDocument(PrecedenceState state)
    {
        var doc = Document<T>.Concat(
            _expressions.Select(e => e.ToDocument(new PrecedenceState(_precedence, false)))
        );

        var shouldParenthesise = _precedence < state.CurrentPrecedence
            || (state.CurrentPrecedence == _precedence && state.PrecedenceTiebreak);

        return shouldParenthesise
            ? _bracketer.Bracket(doc)
            : doc;
    }
}

internal class DocumentExpression<T> : Expression<T>
{
    private readonly Document<T> _document;

    public DocumentExpression(Document<T> document)
    {
        _document = document;
    }

    internal override Document<T> ToDocument(PrecedenceState state)
        => _document;
}

internal class BumpedExpression<T> : Expression<T>
{
    private readonly Expression<T> _expr;

    public BumpedExpression(Expression<T> expr)
    {
        _expr = expr;
    }

    internal override Document<T> ToDocument(PrecedenceState state)
        => _expr.ToDocument(state with { PrecedenceTiebreak = true });
}

internal record struct PrecedenceState(
    int CurrentPrecedence,
    bool PrecedenceTiebreak
)
{
    public static PrecedenceState Default { get; }
        = new(0, false);
}
