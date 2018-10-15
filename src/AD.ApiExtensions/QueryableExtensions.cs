using System;
using System.Linq;
using System.Linq.Expressions;
using AD.ApiExtensions.Expressions.Internal;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Extension methods for <see cref="IQueryable"/> and <see cref="IQueryable{T}"/>.
    /// </summary>
    [PublicAPI]
    public static class QueryableExtensions
    {
        #region ExpressionVisitors

        /// <summary>
        /// Enlists an expression visitor for the <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="queryable">The expression tree to visit.</param>
        /// <typeparam name="TVisitor">The type of visitor.</typeparam>
        /// <returns>
        /// The visited queryable.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryable"/></exception>
        /// <exception cref="InvalidOperationException">The enlisted visitor returned null.</exception>
        [Pure]
        [NotNull]
        public static IQueryable Enlist<TVisitor>([NotNull] this IQueryable queryable)
            where TVisitor : ExpressionVisitor, new()
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));

            Expression expression = new TVisitor().Visit(queryable.Expression);

            if (expression == null)
                throw new InvalidOperationException("The enlisted visitor returned null.");

            return queryable.Provider.CreateQuery(expression);
        }

        /// <summary>
        /// Enlists an expression visitor for the <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <param name="queryable">The expression tree to visit.</param>
        /// <typeparam name="TVisitor">The type of visitor </typeparam>
        /// <typeparam name="TInput">The element type of the source.</typeparam>
        /// <typeparam name="TOutput">The element type of the result.</typeparam>
        /// <returns>
        /// The visited queryable.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryable"/></exception>
        /// <exception cref="InvalidOperationException">The enlisted visitor returned null.</exception>
        [Pure]
        [NotNull]
        public static IQueryable<TOutput> Enlist<TVisitor, TInput, TOutput>([NotNull] this IQueryable<TInput> queryable)
            where TVisitor : ExpressionVisitor, new()
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));

            Expression expression = new TVisitor().Visit(queryable.Expression);

            if (expression == null)
                throw new InvalidOperationException("The enlisted visitor returned null.");

            return queryable.Provider.CreateQuery<TOutput>(expression);
        }

        #endregion

        #region With

        /// <summary>
        /// Projects the <see cref="IQueryable{T}"/> and rebinds the target with the specified value.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="target">The property to rebind.</param>
        /// <param name="value">The value to bind to the <paramref name="target"/>.</param>
        /// <typeparam name="TSource">The element type from <paramref name="source"/>.</typeparam>
        /// <typeparam name="TValue">The type of the target property.</typeparam>
        /// <returns>
        /// A query projection in which the <paramref name="value"/> is bound to the <paramref name="target"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="target"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/></exception>
        [Pure]
        [NotNull]
        [LinqTunnel]
        public static IQueryable<TSource> With<TSource, TValue>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TValue>> target,
            [NotNull] Expression<Func<TSource, TValue>> value)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Expression expression =
                new WithExpression<TSource, TValue>(source.Expression, target, value);

            return source.Provider.CreateQuery<TSource>(expression.Reduce());
        }

        #endregion
    }
}