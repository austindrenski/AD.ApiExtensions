using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Visitors
{
    /// <summary>
    /// Represents an expression visitor that preserves type information.
    /// </summary>
    /// <typeparam name="T">
    /// The type preserved by the expression visitor.
    /// </typeparam>
    [PublicAPI]
    // ReSharper disable once UnusedTypeParameter
    public interface ITypedExpressionVisitor<T>
    {
        /// <summary>
        /// Gets the query provider associated with the data source.
        /// </summary>
        [NotNull]
        IQueryProvider Provider { get; }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        [NotNull]
        Expression Visit(Expression node);
    }
}