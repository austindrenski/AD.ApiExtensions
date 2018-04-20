using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Visitors
{
    /// <summary>
    /// Extension methods to enlist expression visitors on the preceeding tree.
    /// </summary>
    [PublicAPI]
    public static class EnlistExpressionVisitor
    {
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
        public static IQueryable Enlist<TVisitor>([NotNull] this IQueryable queryable) where TVisitor : ExpressionVisitor, new()
        {
            if (queryable is null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            TVisitor visitor = new TVisitor();

            if (!(visitor.Visit(queryable.Expression) is Expression expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

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
        public static IQueryable<T> Enlist<T, TVisitor>([NotNull] this IQueryable<T> queryable) where TVisitor : ITypedExpressionVisitor<T>, new()
        {
            if (queryable is null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            return queryable.Provider.CreateQuery<T>(new TVisitor().Visit(queryable.Expression));
        }
    }
}