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
        [NotNull]
        protected override Expression VisitLambda<T>([NotNull] Expression<T> node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            ParameterExpression[] parameterExpression =
                node.Parameters
                    .Select(Visit)
                    .Cast<ParameterExpression>()
                    .ToArray();

            Expression body = Visit(node.Body);

            return Expression.Lambda(body, parameterExpression);
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        protected override Expression VisitMember([NotNull] MemberExpression node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            // TODO: take another look at this.
            if (node.Expression is MemberExpression innerMemberExpression)
            {
                return
                    _cache.TryGetParameter(innerMemberExpression.Member.DeclaringType, out ParameterExpression test)
                        ? Expression.PropertyOrField(
                            Expression.PropertyOrField(
                                test,
                                innerMemberExpression.Member.Name),
                            node.Member.Name)
                        : base.VisitMember(node);
            }
            else
            {
                return
                    _cache.TryGetParameter(node.Member.DeclaringType, out ParameterExpression test)
                        ? Expression.PropertyOrField(test, node.Member.Name)
                        : base.VisitMember(node);
            }
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        protected override Expression VisitMethodCall([NotNull] MethodCallExpression node)
        {
            if (node is null)
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
        [NotNull]
        protected override Expression VisitNew([NotNull] NewExpression node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (node.Arguments is null || node.Arguments.Count is 0)
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
                if (!assignments.Any(x => _cache.ContainsKey(x.Member.DeclaringType)))
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
                    Expression.New(next.GetConstructor(Type.EmptyTypes)),
                    assignments.Select(x => Expression.Bind(next.GetProperty(x.Member.Name, BindingFlags.Instance | BindingFlags.Public), x.Argument)));
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        protected override Expression VisitParameter([NotNull] ParameterExpression node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return _cache.TryGetParameter(node.Type, out ParameterExpression result) ? result : base.VisitParameter(node);
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        protected override Expression VisitUnary([NotNull] UnaryExpression node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Expression operand = Visit(node.Operand);

            // TODO: Is this universal? Recursive?
            // Lifting results that are double quoted.
            if (node is UnaryExpression unary && unary.NodeType == ExpressionType.Quote)
            {
                operand = Visit(unary.Operand);
            }

            return Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method);
        }
    }
}