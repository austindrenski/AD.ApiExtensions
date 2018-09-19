using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AD.ApiExtensions.Types;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Visitors
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an expression visitor which reduces empty statements dependant upon constant values.
    /// </summary>
    [PublicAPI]
    public sealed class ProjectionEliminatingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Caches anonymous types that were encountered and modified.
        /// </summary>
        [NotNull] readonly TypeCache _knownTypes = new TypeCache();

        /// <summary>
        /// Caches members that were encountered and removed.
        /// </summary>
        [NotNull] readonly IDictionary<string, MemberInfo> _eliminatedMembers = new Dictionary<string, MemberInfo>();

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitLambda<T>(Expression<T> e)
        {
            Expression body = Visit(e.Body);

            ParameterExpression[] parameters = e.Parameters.ToArray();

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!_knownTypes.TryGetParameter(parameters[i].Type, out ParameterExpression result))
                    continue;

                ParameterReplacingExpressionVisitor visitor =
                    new ParameterReplacingExpressionVisitor(parameters[i], result);

                body = visitor.Visit(body);
                parameters[i] = result;
            }

            return body != null ? Expression.Lambda(body, parameters) : base.VisitLambda(e);
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitMember(MemberExpression e)
        {
            // TODO: take another look at this.
            if (e.Expression is MemberExpression inner)
            {
                return
                    _knownTypes.TryGetParameter(inner.Member.DeclaringType, out ParameterExpression test)
                        ? Expression.PropertyOrField(Expression.PropertyOrField(test, inner.Member.Name), e.Member.Name)
                        : base.VisitMember(e);
            }

            return
                _knownTypes.TryGetParameter(e.Member.DeclaringType, out ParameterExpression parameter)
                    ? Expression.PropertyOrField(parameter, e.Member.Name)
                    : base.VisitMember(e);
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression e)
        {
            // N.B. materialization order matters.
            Expression instance = Visit(e.Object);
            Expression[] arguments = e.Arguments.Select(Visit).ToArray();

            MethodInfo method =
                e.Method.IsGenericMethod
                    ? e.Method
                       .GetGenericMethodDefinition()
                       .MakeGenericMethod(e.Method.GetGenericArguments().Select(_knownTypes.GetOrUpdate).ToArray())
                    : e.Method;

            // TODO: need to allow flexibility between interfaces
            ParameterInfo[] paramInfo = method.GetParameters();

            for (int i = 0; i < paramInfo.Length; i++)
            {
                if (paramInfo[i].ParameterType.IsAssignableFrom(arguments[i].Type))
                    continue;

                arguments[i] = Expression.Convert(arguments[i], paramInfo[i].ParameterType);

//                    Expression.MakeUnary(
//                        arguments[i].Type.IsValueType ? ExpressionType.Convert : ExpressionType.TypeAs,
//                        arguments[i],
//                        paramInfo[i].ParameterType);
            }

            return Expression.Call(instance, method, arguments);
        }

        /// <inheritdoc />
        protected override Expression VisitNew(NewExpression e)
        {
            if (e.Arguments.Count == 0)
                return base.VisitNew(e);

            (MemberInfo Member, Expression Argument)[] assignments =
                e.Arguments
                 .Zip(e.Members, (a, m) => (Member: m, Argument: a))
                 .Where(x => !_knownTypes.IsLogicallyDefault(x.Member, x.Argument, _eliminatedMembers))
                 .Select(x => (x.Member, Visit(x.Argument)))
                 .ToArray();

            if (assignments.Length == e.Arguments.Count)
            {
                if (!assignments.Any(x => x.Member.DeclaringType is Type type && _knownTypes.ContainsKey(type)))
                {
                    if (assignments.Zip(e.Arguments, (x, y) => x.Argument == y).All(x => x))
                        return e.Update(assignments.Select(x => x.Argument));
                }
            }

            // TODO: This is the current "unavailable" methodology. Fix this later.
            IEnumerable<string> toAdd =
                e.Members
                 .Select(x => x.Name)
                 .Except(assignments.Select(x => x.Member.Name))
                 .Except(_eliminatedMembers.Values.Select(x => x.Name));

            foreach (string removed in toAdd)
            {
                _eliminatedMembers.Add(removed, e.Members.Single(x => x.Name == removed));
            }

            Type next = TypeDefinition.GetOrAdd(assignments.Select(x => (x.Member.Name, x.Argument.Type)));

            _knownTypes.Register(e.Type, next);

            ConstructorInfo ctor =
                next.GetConstructor(assignments.Select(x => ((PropertyInfo) x.Member).PropertyType).ToArray());

            if (ctor == null)
                throw new MissingMemberException($"Constructor not found for {next.FullName} with {assignments.Length} parameters.");

            return Expression.New(ctor, assignments.Select(x => x.Argument));
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitParameter(ParameterExpression e)
            => _knownTypes.TryGetParameter(e.Type, out ParameterExpression p) ? p : base.VisitParameter(e);

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitUnary(UnaryExpression e)
            => Visit(e.Operand) is Expression operand
                ? Expression.MakeUnary(e.NodeType, operand, e.Type, e.Method)
                : base.VisitUnary(e);
    }
}