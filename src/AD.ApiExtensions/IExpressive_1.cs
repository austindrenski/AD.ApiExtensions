using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a type that provides a method to reveal an expression-based representation. 
    /// </summary>
    /// <typeparam name="T">
    /// The type of value that the expression returns.
    /// </typeparam>
    [PublicAPI]
    public interface IExpressive<T> : IExpressive
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
        T Express(Expression<Func<T>> argument);

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
        Expression Reduce([NotNull] Expression<Func<T>> argument);
    }
}