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
        [NotNull] readonly Dictionary<string, MemberInfo> _eliminatedMembers = new Dictionary<string, MemberInfo>();

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

            Type[] typeArguments =
                parameters.Select(x => x.Type)
                          .Append(NegotiateMutualInterface(e.ReturnType, body.Type))
                          .ToArray();

            Type funcType = Expression.GetFuncType(typeArguments);

            return Expression.Lambda(funcType, body, parameters);
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

            MethodInfo genericMethod =
                e.Method.GetGenericMethodDefinition();

            Type[] typeParameters = genericMethod.GetGenericArguments();
            ParameterInfo[] parameters = genericMethod.GetParameters();

            for (int i = 0; i < typeParameters.Length; i++)
            {
                Type typeParameter = typeParameters[i];
                for (int j = 0; j < parameters.Length; j++)
                {
                    // N.B. Continue the loop in case a later parameter is a better fit.
                    if (TryInferGenericArgument(typeParameter, parameters[j].ParameterType, arguments[j].Type, out Type result))
                        typeParameters[i] = _knownTypes.GetOrUpdate(result);
                }
            }

            MethodInfo method = genericMethod.MakeGenericMethod(typeParameters);

            return Expression.Call(instance, method, arguments);
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

        /// <summary>
        /// Defines a new type and constructs a <see cref="NewExpression"/> for it.
        /// </summary>
        /// <param name="type">The type that the new type replaces.</param>
        /// <param name="assignments">The assignments representing the new type.</param>
        /// <returns>
        /// A <see cref="NewExpression"/> for the new type.
        /// </returns>
        /// <exception cref="MissingMemberException">Constructor not found for {next.FullName} with {assignments.Length} parameters.</exception>
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

        /// <summary>
        /// Attempts to infer the concrete type used in place of a specific type parameter,
        /// such as the concrete type of `TKey` in <see cref="IGrouping{TKey,TValue}"/>.
        /// </summary>
        /// <param name="typeParameter">The type parameter being inferred.</param>
        /// <param name="parameter">The parameter of the generic method.</param>
        /// <param name="argument">The argument to the current method.</param>
        /// <param name="result">The inferred result.</param>
        /// <returns>
        /// True if a type was inferred; otherwise false.
        /// </returns>
        [ContractAnnotation("=> false, result:null; => true, result:notnull")]
        static bool TryInferGenericArgument([NotNull] Type typeParameter, [NotNull] Type parameter, [NotNull] Type argument, out Type result)
        {
            // N.B. consider T[] == IEnumerable<T>
            if (parameter.IsGenericType && argument.IsGenericType || argument.IsArray)
            {
                Type[] parameterTypeArguments =
                    parameter.GetGenericArguments();

                Type[] argumentTypeArguments =
                    argument.IsGenericType
                        ? argument.GetGenericArguments()
                        : argument.IsArray
                            ? new[] { argument.GetElementType() }
                            : new Type[0];

                for (int i = 0; i < parameterTypeArguments.Length; i++)
                {
                    if (argumentTypeArguments.Length <= i)
                        throw new ArgumentOutOfRangeException("The argument has fewer type parameters than the method parameter.");

                    if (typeParameter.IsAssignableFrom(parameterTypeArguments[i]))
                    {
                        result = argumentTypeArguments[i];
                        return true;
                    }

                    if (TryInferGenericArgument(typeParameter, parameterTypeArguments[i], argumentTypeArguments[i], out result))
                        return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Finds an interface such that the <paramref name="value"/> fits the shape of the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The shape of the type to target.</param>
        /// <param name="value">The type from which to find a matching interface.</param>
        /// <returns>
        /// A constructed interface type for <paramref name="value"/> that fits the shape of the <paramref name="target"/>.
        /// The <paramref name="value"/> is returned if no interface is found.
        /// </returns>
        [Pure]
        [NotNull]
        static Type NegotiateMutualInterface([NotNull] Type target, [NotNull] Type value)
        {
            if (!target.IsInterface)
                return value;

            Type targetGeneric =
                target.IsConstructedGenericType
                    ? target.GetGenericTypeDefinition()
                    : target;

            Type[] interfaces = value.GetInterfaces();
            Type[] interfaceGenerics = new Type[interfaces.Length];

            for (int i = 0; i < interfaces.Length; i++)
            {
                interfaceGenerics[i] =
                    interfaces[i].IsGenericType
                        ? interfaces[i].GetGenericTypeDefinition()
                        : interfaces[i];
            }

            // Look for an exact match.
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (targetGeneric == interfaceGenerics[i])
                    return interfaces[i];
            }

            // Look for an acceptable match.
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (targetGeneric.IsAssignableFrom(interfaceGenerics[i]))
                    return interfaces[i];
            }

            // No match, just return the value.
            return value;
        }
    }
}