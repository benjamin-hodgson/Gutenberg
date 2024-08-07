using static Gutenberg.Tests.DocumentTestUtil;

using BinOp = Gutenberg.Expression.BinaryOperator<object>;
using Op = Gutenberg.Expression.OperatorFactory<object>;
using UnOp = Gutenberg.Expression.UnaryOperator<object>;

namespace Gutenberg.Tests;

public class ExpressionTests
{
    private static readonly UnOp _not = Op.Prefix(6, "!", chainable: true);
    private static readonly BinOp _plus = Op.InfixL(5, " + ");
    private static readonly UnOp _incr = Op.Postfix(5, "++");
    private static readonly BinOp _eq = Op.InfixL(4, " == ");
    private static readonly BinOp _arr = Op.InfixR(3, " -> ");

    [Fact]
    public void TestInfixL()
    {
        var expr = _plus.Apply(
            _plus.Apply("x", "3"),
            _plus.Apply(_plus.Apply("5", "y"), "7")
        );
        TestDocument("x + 3 + (5 + y + 7)", expr.PrettyPrint());
    }

    [Fact]
    public void TestInfixR()
    {
        var expr = _arr.Apply(
            _arr.Apply("Int", "Bool"),
            _arr.Apply("[Int]", _arr.Apply("Int", "[Int]"))
        );
        TestDocument("(Int -> Bool) -> [Int] -> Int -> [Int]", expr.PrettyPrint());
    }

    [Fact]
    public void TestPrefixChainable()
    {
        var expr = _not.Apply(_not.Apply(_eq.Apply("x", "y")));
        TestDocument("!!(x == y)", expr.PrettyPrint());
    }

    [Fact]
    public void TestPostfixNonChainable()
    {
        var expr = _incr.Apply(_incr.Apply("x"));
        TestDocument("(x++)++", expr.PrettyPrint());
    }
}
