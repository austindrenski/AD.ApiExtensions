using System.Linq;
using AD.ApiExtensions.Visitors;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [UsedImplicitly]
    public class TypeCacheTest
    {
        [Fact]
        public void GroupByUpdated()
        {
            var values = new[] { new { A = "a", B = "b", C = "c" } };

            var group =
                values.AsQueryable()
                      .Select(x => new { x.A, x.B, D = 1 })
                      .GroupBy(x => new { x.A, x.B })
                      .Select(x => new { x.Key.A, x.Key.B, D = x.Sum(y => y.D) })
                      .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void GroupByUpdated_with_avoided_elimination()
        {
            var values = new[] { new { A = "a", B = "b", C = "c" } };

            var group =
                values.AsQueryable()
                      .Select(x => new { x.A, x.B, D = 0 })
                      .GroupBy(x => new { x.A, x.B })
                      .Select(x => new { x.Key.A, x.Key.B, D = x.Sum(y => y.D) })
                      .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void GroupByUpdated_with_elimination()
        {
            var values = new[] { new { A = "a", B = "b", C = "c" } };

            var group =
                values.AsQueryable()
                      .Select(x => new { x.A, x.B, D = 0 })
                      .GroupBy(x => new { x.A, x.B })
                      .SelectMany(x => x)
                      .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }
    }
}