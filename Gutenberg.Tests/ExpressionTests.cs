// using Gutenberg.Expression;
using Op = Gutenberg.Expression.OperatorFactory<object>;
using BinOp = Gutenberg.Expression.BinaryOperator<object>;
using UnOp = Gutenberg.Expression.UnaryOperator<object>;
// using Expr = Gutenberg.Expression.Expression<object>;
// using Doc = Gutenberg.Document<object>;
using static Gutenberg.Tests.DocumentTestUtil;

namespace Gutenberg.Tests;

public class ExpressionTests
{
    private static readonly UnOp _not = Op.Prefix(6, "!");
    private static readonly BinOp _plus = Op.InfixL(5, " + ");
    // private static readonly BinOp _minus = Op.InfixL(5, "-");
    private static readonly BinOp _gt = Op.InfixL(4, " > ");
    // private static readonly BinOp _ge = Op.InfixL(4, ">=");
    // private static readonly BinOp _lt = Op.InfixL(4, "<");
    // private static readonly BinOp _le = Op.InfixL(4, "<=");
    // private static readonly BinOp _eq = Op.InfixL(3, "==");
    // private static readonly BinOp _ne = Op.InfixL(3, "!=");
    // private static readonly BinOp _and = Op.InfixL(2, "&&");
    // private static readonly BinOp _or = Op.InfixL(1, "||");


    [Fact]
    public async Task TestExpression()
    {
        var expr = _not.Apply(_gt.Apply(_plus.Apply("x", "3"), "5"));
        await TestDocument("!(x + 3 > 5)", expr.PrettyPrint());
    }
}
