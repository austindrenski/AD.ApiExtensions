using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Visitors
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an expression visitor which replaces one <see cref="ParameterExpression"/> with another.
    /// </summary>
    [PublicAPI]
    public class ParameterReplacingExpressionVisitor : ExpressionVisitor
    {
        [NotNull] readonly ParameterExpression _source;
        [NotNull] readonly ParameterExpression _result;

        /// <summary>
        /// Constructs a new instance of the <see cref="ParameterReplacingExpressionVisitor"/> class.
        /// </summary>
        /// <param name="source">The parameter to be replaced.</param>
        /// <param name="result">The parameter for replacement.</param>
        public ParameterReplacingExpressionVisitor([NotNull] ParameterExpression source, [NotNull] ParameterExpression result)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (result is null)
                throw new ArgumentNullException(nameof(result));

            _source = source;
            _result = result;
        }

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression e)
            => e == _source ? _result : base.VisitParameter(e);
    }
}