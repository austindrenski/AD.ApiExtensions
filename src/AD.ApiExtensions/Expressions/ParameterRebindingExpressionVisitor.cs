using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    /// <summary>
    /// An expression visitor that rebinds existing parameters of the same type.
    /// </summary>
    [PublicAPI]
    public class ParameterRebindingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// The new parameter.
        /// </summary>
        [NotNull] readonly ParameterExpression _parameter;

        /// <summary>
        /// Constructs a new instance of the <see cref="ParameterRebindingExpressionVisitor"/> class.
        /// </summary>
        /// <param name="parameter">The parameter used for rebinding existing parameters of the same type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"></paramref></exception>
        public ParameterRebindingExpressionVisitor([NotNull] ParameterExpression parameter)
        {
            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            _parameter = parameter;
        }

        /// <inheritdoc />
        [Pure]
        [ContractAnnotation("e:notnull => notnull; e:null => null")]
        public override Expression Visit(Expression e) => base.Visit(e);

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitParameter(ParameterExpression e)
            => e.Type == _parameter.Type ? _parameter : base.VisitParameter(e);
    }
}