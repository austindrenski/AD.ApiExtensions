using System.Linq;
using AD.ApiExtensions.Expressions;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [UsedImplicitly]
    public class ProjectionEliminatingExpressionVisitorTest
    {
        #region Contains

        [Fact]
        public void Contains_anonymous()
        {
            var strings = new[] { "a", "b", "c" };
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new { x.A, x.B, D = 1 })
                     .Where(x => strings.Contains(x.A))
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D })
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
        public void GroupBy_anonymous_with_elimination()
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

        [Fact(Skip = "WIP")]
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
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1 })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void Select_concrete_with_elimination()
        {
            var query = new[] { new { A = "a", B = "b", C = "c" } }.AsQueryable();

            var select =
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 0 })
                     .Select(x => new SomeClass { A = x.A, B = x.B, D = x.D })
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
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
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(3, result.First().GetType().GetProperties().Length);
        }

        [Fact]
        public void With_concrete_twice()
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
                query.Select(x => new SomeClass { A = x.A, B = x.B, D = 1 })
                     .With(x => x.D, x => 0)
                     .Enlist<ProjectionEliminatingExpressionVisitor>();

            var result = select.Cast<object>().ToArray();

            Assert.Equal(2, result.First().GetType().GetProperties().Length);
        }

        #endregion

        #region Support

        class SomeClass
        {
            public string A { get; set; }
            public string B { get; set; }
            public int D { get; set; }
        }

        #endregion
    }
}