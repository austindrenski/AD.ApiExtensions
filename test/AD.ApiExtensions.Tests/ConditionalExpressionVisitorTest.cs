using System.Linq.Expressions;
using AD.ApiExtensions.Expressions;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [UsedImplicitly]
    public class ConditionalExpressionVisitorTest
    {
        [Fact]
        public void Test0()
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