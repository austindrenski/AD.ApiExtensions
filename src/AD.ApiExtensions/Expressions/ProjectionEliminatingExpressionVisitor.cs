using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AD.ApiExtensions.Types;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
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
        [ContractAnnotation("e:notnull => notnull; e:null => null")]
        public override Expression Visit(Expression e) => base.Visit(e);

        /// <inheritdoc />
        protected override Expression VisitLambda<T>(Expression<T> e)
        {
            Expression body = Visit(e.Body);

            ParameterExpression[] parameters =
                e.Parameters
                 .Select(Visit)
                 .Cast<ParameterExpression>()
                 .ToArray();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterReplacingExpressionVisitor visitor =
                    new ParameterReplacingExpressionVisitor(e.Parameters[i], parameters[i]);

                body = visitor.Visit(body);
            }

            // TODO: make this less brittle.
            Type[] typeArgs = new Type[parameters.Length + 1];

            for (int i = 0; i < parameters.Length; i++)
            {
                typeArgs[i] = parameters[i].Type;
            }

            typeArgs[parameters.Length] = body.Type;

            Type delegateType = Expression.GetDelegateType(typeArgs);

            // TODO: needs fixed for the general case.
            if (body.Type.IsGenericType &&
                body.Type.GetGenericTypeDefinition() != delegateType.GetGenericTypeDefinition())
            {
                // TODO: obviously too brittle.
                body = Expression.TypeAs(body, body.Type.GetInterfaces()[0]);
                typeArgs[parameters.Length] = body.Type;
                delegateType = Expression.GetDelegateType(typeArgs);
            }

            return Expression.Lambda(delegateType, body, parameters);
        }

        /// <inheritdoc />
        protected override Expression VisitMember(MemberExpression e)
        {
            switch (Visit(e.Expression))
            {
                case ParameterExpression p:
                    return Expression.PropertyOrField(VisitParameter(p), e.Member.Name);

                case Expression s:
                    return Expression.PropertyOrField(s, e.Member.Name);

                default:
                    return base.VisitMember(e);
            }
        }

        /// <inheritdoc />
        protected override Expression VisitMemberInit(MemberInitExpression e)
        {
            if (!(Visit(e.NewExpression) is NewExpression newExpression))
                return base.VisitMemberInit(e);

            MemberBinding[] bindings =
                e.Bindings
                 .Where(x => x.Member.DeclaringType != null)
                 .Where(x => _knownTypes.GetOrUpdate(x.Member.DeclaringType).GetRuntimeProperties().Any(y => y.Name == x.Member.Name))
                 .ToArray();

            // TODO: handle MemberBindingType.ListBinding and MemberBindingType.MemberBinding.
            if (bindings.Any(x => x.BindingType != MemberBindingType.Assignment))
                return e.Update(newExpression, bindings.Select(VisitMemberBinding));

            (string Name, Expression Expression)[] assignments =
                bindings.Cast<MemberAssignment>()
                        .Where(x => !_knownTypes.IsLogicallyDefault(x.Member, x.Expression, _eliminatedMembers))
                        .Select(x => (x.Member.Name, Visit(x.Expression)))
                        .ToArray();

            return ConstructNewTypeAndExpression(e.Type, assignments);
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression e)
        {
            Expression instance = Visit(e.Object);
            Expression[] arguments = e.Arguments.Select(Visit).ToArray();

            if (!e.Method.IsGenericMethod)
                return e.Update(instance, arguments);

            MethodInfo generic =
                e.Method.GetGenericMethodDefinition();

            Type[] genericArguments = generic.GetGenericArguments();
            ParameterInfo[] genericParameters = generic.GetParameters();

            Type[] types = new Type[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                Type genericType = genericArguments[i];
                for (int j = 0; j < genericParameters.Length; j++)
                {
                    Type parameterType = genericParameters[j].ParameterType;
                    Type realArgType = arguments[j].Type;

                    if (TryInfer(genericType, parameterType, realArgType, out Type result))
                        types[i] = result;
                }
            }

            MethodInfo method = generic.MakeGenericMethod(types);

            return Expression.Call(instance, method, arguments);
        }

        bool TryInfer(Type genericType, Type parameterType, Type realArgType, out Type result)
        {
            if (parameterType.IsGenericType)
            {
                Type[] parameterTypeGenerics = parameterType.GetGenericArguments();
                for (int i = 0; i < parameterTypeGenerics.Length; i++)
                {
                    Type innerParameterTypeGeneric = parameterTypeGenerics[i];
                    Type innerRealArgType = realArgType.GetGenericArguments()[i];

                    if (genericType.IsAssignableFrom(innerParameterTypeGeneric))
                    {
                        result = _knownTypes.GetOrUpdate(innerRealArgType);
                        return true;
                    }

                    if (TryInfer(genericType, innerParameterTypeGeneric, innerRealArgType, out result))
                        return true;
                }
            }

            result = null;
            return false;
        }

        /// <inheritdoc />
        protected override Expression VisitNew(NewExpression e)
        {
            if (e.Arguments.Count == 0 || e.Members == null)
                return base.VisitNew(e);

            (string Name, Expression)[] assignments =
                e.Arguments
                 .Zip(e.Members, (a, m) => (Member: m, Expression: a))
                 .Where(x => !_knownTypes.IsLogicallyDefault(x.Member, x.Expression, _eliminatedMembers))
                 .Select(x => (x.Member.Name, Visit(x.Expression)))
                 .ToArray();

            // TODO: This is the current "unavailable" methodology. Fix this later.
            IEnumerable<string> toAdd =
                e.Members
                 .Select(x => x.Name)
                 .Except(assignments.Select(x => x.Name))
                 .Except(_eliminatedMembers.Values.Select(x => x.Name));

            foreach (string removed in toAdd)
            {
                _eliminatedMembers.Add(removed, e.Members.Single(x => x.Name == removed));
            }

            return ConstructNewTypeAndExpression(e.Type, assignments);
        }

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression e)
        {
            if (_knownTypes.TryGetValue(e.Type, out ParameterExpression p))
                return p;

            Type type = _knownTypes.GetOrUpdate(e.Type);

            if (e.Type != type)
                _knownTypes.Register(e.Type, type);

            return _knownTypes.TryGetValue(type, out p) ? p : base.VisitParameter(e);
        }

        [NotNull]
        Expression ConstructNewTypeAndExpression([NotNull] Type type, [NotNull] (string Name, Expression Expression)[] assignments)
        {
            Type next = TypeDefinition.GetOrAdd(assignments.Select(x => (x.Name, x.Expression.Type)));

            _knownTypes.Register(type, next);

            ConstructorInfo ctor = next.GetConstructor(assignments.Select(x => x.Expression.Type).ToArray());

            if (ctor == null)
                throw new MissingMemberException($"Constructor not found for {next.FullName} with {assignments.Length} parameters.");

            return Expression.New(
                ctor,
                assignments.Select(x => x.Expression),
                next.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(x => assignments.Any(y => y.Name == x.Name)));
        }
    }
}