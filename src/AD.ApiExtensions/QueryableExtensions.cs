using System;
using System.Linq;
using System.Linq.Expressions;
using AD.ApiExtensions.Visitors;
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
        /// Enlists an expression visitor on the non-generic queryable.
        /// </summary>
        /// <param name="queryable">
        /// The expression tree to visit.
        /// </param>
        /// <returns>
        /// The visited queryable.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException" />
        [Pure]
        [NotNull]
        public static IQueryable Enlist<TVisitor>([NotNull] this IQueryable queryable)
            where TVisitor : ExpressionVisitor, new()
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));

            Expression expression = new TVisitor().Visit(queryable.Expression);

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return queryable.Provider.CreateQuery(expression);
        }

        /// <summary>
        /// Enlists an expression visitor on the generic queryable.
        /// </summary>
        /// <param name="queryable">
        /// The expression tree to visit.
        /// </param>
        /// <returns>
        /// The visited queryable.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException" />
        [Pure]
        [NotNull]
        public static IQueryable<T> Enlist<T, TVisitor>([NotNull] this IQueryable<T> queryable)
            where TVisitor : ITypedExpressionVisitor<T>, new()
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));

            return queryable.Provider.CreateQuery<T>(new TVisitor().Visit(queryable.Expression));
        }

        #endregion
    }
}