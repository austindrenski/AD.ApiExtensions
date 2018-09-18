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
                      .Enlist<ProjectionEliminationExpressionVisitor>();

            var _ = group.Cast<object>().ToArray();
        }
    }
}