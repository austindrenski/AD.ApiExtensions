using System.Linq.Expressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Represents a type that provides a method to reveal an expression-based representation.
    /// </summary>
    [PublicAPI]
    public interface IExpressive
    {
        /// <summary>
        /// Expresses the outcome of a function that will be resolved by an expression visitor.
        /// </summary>
        /// <param name="argument">
        /// An expression used in constructing the expression.
        /// </param>
        /// <returns>
        /// The expressed value or throws an exception.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException" />
        [Pure]
        [NotNull]
        object Express(Expression argument);

        /// <summary>
        /// Constructs an expression that returns a value. This should be called by an expression visitor.
        /// </summary>
        /// <param name="argument">
        /// An expression used in constructing the expression.
        /// </param>
        /// <returns>
        /// An expression that provides a value.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException" />
        [Pure]
        [NotNull]
        Expression Reduce([NotNull] Expression argument);
    }
}