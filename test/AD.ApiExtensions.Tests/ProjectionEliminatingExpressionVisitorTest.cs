using System.Linq;
using AD.ApiExtensions.Expressions;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [UsedImplicitly]
    public class ProjectionEliminatingExpressionVisitorTest
    {
        #region Cast

        [Fact]
        public void Cast_anonymous()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .Cast<object>()
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Cast_anonymous_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .Select(x => new { x.A, x.B, D = 0 })
                     .Cast<object>()
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Cast_concrete()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1, Other = new SomeOtherClass { E = "e" } })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Cast<object>()
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(4, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Cast_concrete_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c", } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 0, Other = new SomeOtherClass { E = "e" } })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Cast<object>()
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        #endregion

        #region Contains

        [Fact]
        public void Contains_anonymous()
        {
            var strings = new[] { "a", "b", "c" };
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .Where(x => strings.Contains(x.A))
                     .Select(x => new { x.A, x.B, x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Contains_anonymous_with_elimination()
        {
            var strings = new[] { "a", "b", "c" };
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 0 })
                     .Where(x => strings.Contains(x.A))
                     .Select(x => new { x.A, x.B, x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Contains_concrete()
        {
            var strings = new[] { "a", "b", "c" };
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1 })
                     .Where(x => strings.Contains(x.A))
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Contains_concrete_with_elimination()
        {
            var strings = new[] { "a", "b", "c" };
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 0 })
                     .Where(x => strings.Contains(x.A))
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        #endregion

        #region GroupBy

        [Fact]
        public void GroupBy_anonymous()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var group =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .GroupBy(x => new { x.A, x.B })
                     .Select(x => new { x.Key.A, x.Key.B, D = x.Sum(y => y.D) })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void GroupBy_anonymous_with_elimination_average()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var group =
                query.Select(x => new { x.A, x.B, D = 0.0 })
                     .GroupBy(x => new { x.A, x.B })
                     .Select(x => new { x.Key.A, x.Key.B, D = x.Average(y => y.D) })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void GroupBy_anonymous_with_elimination_sum()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var group =
                query.Select(x => new { x.A, x.B, D = 0 })
                     .GroupBy(x => new { x.A, x.B })
                     .Select(x => new { x.Key.A, x.Key.B, D = x.Sum(y => y.D) })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void GroupBy_anonymous_with_elimination_and_implicit_cast()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var group =
                query.Select(x => new { x.A, x.B, D = 0 })
                     .GroupBy(x => new { x.A, x.B })
                     .SelectMany(x => x)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        #endregion

        #region Join

        [Fact]
        public void Join_anonymous()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var group =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .Join(query, x => x.A, x => x.A, (x, y) => new { x, y })
                      // ReSharper disable once EqualExpressionComparison
                     .Where(x => x.x == x.x)
                     .Select(x => new { x.x.A, x.x.B, x.y.C, x.x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(4, result.First().GetType().GetProperties().Length);
            Assert.Single(result);
        }

        [Fact]
        public void Join_anonymous_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var group =
                query.Select(x => new { x.A, x.B, D = 0.0 })
                     .Join(query, x => x.A, x => x.A, (x, y) => new { x, y })
                      // ReSharper disable once EqualExpressionComparison
                     .Where(x => x.x == x.x)
                     .Select(x => new { x.x.A, x.x.B, x.y.C, x.x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = group.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
            Assert.Single(result);
        }

        #endregion

        #region Select

        [Fact]
        public void Select_anonymous()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .Select(x => new { x.A, x.B, x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Select_anonymous_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 0 })
                     .Select(x => new { x.A, x.B, x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Select_concrete()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1, Other = new SomeOtherClass { E = "e" } })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(4, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Select_concrete_then_anonymous()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1, Other = new SomeOtherClass { E = "e" } })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Select(x => new { x.A, x.B, x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(4, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Select_concrete_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c", } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 0, Other = new SomeOtherClass { E = "e" } })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Select_concrete_with_elimination_then_anonymous_()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 0, Other = new SomeOtherClass { E = "e" } })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Select(x => new { x.A, x.B, x.D, Other = new SomeOtherClass { E = x.Other.E } })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        #endregion

        #region With

        [Fact]
        public void With_anonymous()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .With(x => x.D, x => x.D)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void With_anonymous_twice()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .With(x => x.D, x => x.D)
                     .With(x => x.A, x => x.A)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void With_anonymous_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .With(x => x.D, x => 0)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void With_concrete()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1 })
                     .With(x => x.D, x => x.D)
                     .With(x => x.A, x => x.A)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void With_concrete_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1, Other = new SomeOtherClass { E = "e" } })
                     .With(x => x.D, x => 0)
                     .With(x => x.A, x => x.A)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void With_concrete_with_navigation()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1, Other = new SomeOtherClass { E = "e" } })
                     .With(x => x.Other, x => new SomeOtherClass { E = x.Other.E })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(4, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void With_concrete_with_navigation_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1, Other = new SomeOtherClass { E = "e" } })
                     .With(x => x.Other, x => null)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        #endregion

        #region Support

        class SomeClass
        {
            public string A { get; set; }
            public string B { get; set; }
            public int D { get; set; }
            public SomeOtherClass Other { get; set; }
        }

        class SomeOtherClass
        {
            public string E { get; set; }
        }

        #endregion
    }
}