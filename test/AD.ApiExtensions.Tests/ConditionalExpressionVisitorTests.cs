using System.Linq.Expressions;
using AD.ApiExtensions.Visitors;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [PublicAPI]
    public class ConditionalExpressionVisitorTests
    {
        [Fact]
        public static void Test0()
        {
            Expression expression = Expression.Not(Expression.Constant(true));

            ConditionalExpressionVisitor visitor = new ConditionalExpressionVisitor();

            Expression result = visitor.Visit(expression);

            Assert.IsType<ConstantExpression>(result);
            Assert.IsType<bool>(((ConstantExpression) result).Value);
            Assert.False((bool) ((ConstantExpression) result).Value);
        }
    }
}