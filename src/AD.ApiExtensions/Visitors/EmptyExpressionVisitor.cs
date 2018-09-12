using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AD.ApiExtensions.Expressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Visitors
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an expression visitor which reduces empty statements dependant upon constant values.
    /// </summary>
    [PublicAPI]
    public sealed class EmptyExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Caches anonymous types that were encountered and modified.
        /// </summary>
        [NotNull] private readonly TypeCache _cache;

        /// <summary>
        /// Caches members that were encountered and removed.
        /// </summary>
        [NotNull] private readonly IDictionary<string, MemberInfo> _removedMembers;

        /// <inheritdoc />
        public EmptyExpressionVisitor()
        {
            _cache = new TypeCache();
            _removedMembers = new Dictionary<string, MemberInfo>();
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            ParameterExpression[] parameterExpression =
                node.Parameters
                    .Select(Visit)
                    .Cast<ParameterExpression>()
                    .ToArray();

            if (!(Visit(node.Body) is Expression body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            return Expression.Lambda(body, parameterExpression);
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            // TODO: take another look at this.
            if (node.Expression is MemberExpression innerMemberExpression)
            {
                if (!(innerMemberExpression.Member.DeclaringType is Type type))
                    throw new ArgumentNullException(nameof(type));

                return
                    _cache.TryGetParameter(type, out ParameterExpression test) && test != null
                        ? Expression.PropertyOrField(
                            Expression.PropertyOrField(test, innerMemberExpression.Member.Name),
                            node.Member.Name)
                        : base.VisitMember(node);
            }
            else
            {
                if (!(node.Member.DeclaringType is Type type))
                {
                    throw new ArgumentNullException(nameof(type));
                }

                return
                    _cache.TryGetParameter(type, out ParameterExpression test) && test != null
                        ? Expression.PropertyOrField(test, node.Member.Name)
                        : base.VisitMember(node);
            }
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Expression instance = Visit(node.Object);

            Expression[] arguments =
                node.Arguments
                    .Select(Visit)
                    .Select(
                         x =>
                         {
                             _cache.Register(x.Type, x.Type);
                             return _cache.GetParameterOrInput(x);
                         })
                    .ToArray();

            MethodInfo method = _cache.GetMethodInfoOrInput(node.Method);

            return Expression.Call(instance, method, arguments);
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitNew(NewExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (node.Arguments.Count is 0)
            {
                return node;
            }

            (MemberInfo Member, Expression Argument)[] assignments =
                node.Arguments
                    .Zip(node.Members, (a, m) => (Member: m, Argument: a))
                    .Where(x => !_cache.IsLogicallyDefault(x.Member, x.Argument, _removedMembers))
                    .Select(x => (x.Member, Visit(x.Argument)))
                    .ToArray();

            if (assignments.Length == node.Arguments.Count)
            {
                if (!assignments.Any(x => x.Member.DeclaringType is Type type && _cache.ContainsKey(type)))
                {
                    if (assignments.Zip(node.Arguments, (x, y) => x.Argument == y).All(x => x))
                    {
                        return node.Update(assignments.Select(x => x.Argument));
                    }
                }
            }

            // TODO: This is the current "unavailable" methodology. Fix this later.
            IEnumerable<string> toAdd = node.Members.Select(x => x.Name).Except(assignments.Select(x => x.Member.Name)).Except(_removedMembers.Values.Select(x => x.Name));
            foreach (string removed in toAdd)
            {
                _removedMembers.Add(removed, node.Members.Single(x => x.Name == removed));
            }

            Type next =
                assignments.Select(x => (x.Member.Name, x.Argument.Type))
                           .CreateNew();

            _cache.Register(node.Type, next);

            return
                Expression.MemberInit(
                    Expression.New(next.GetEmptyConstructor()),
                    assignments.Select(x => Expression.Bind(next.GetPropertyInfo(x.Member), x.Argument)));
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return _cache.TryGetParameter(node.Type, out ParameterExpression result) && result != null ? result : base.VisitParameter(node);
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Expression operand = Visit(node.Operand);

            // TODO: Is this universal? Recursive?
            // Lifting results that are double quoted.
            if (node is UnaryExpression unary && unary.NodeType is ExpressionType.Quote)
            {
                operand = Visit(unary.Operand);
            }

            if (operand == null)
            {
                throw new ArgumentNullException(nameof(operand));
            }

            return Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method);
        }
    }
}