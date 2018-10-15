using System;
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

        /// <inheritdoc />
        [ContractAnnotation("e:notnull => notnull; e:null => null")]
        public override Expression Visit(Expression e) => base.Visit(e);

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression e)
            => e.NodeType == ExpressionType.Equal ? Expression.Equal(Visit(e.Left), Visit(e.Right)) : base.VisitBinary(e);

        /// <inheritdoc />
        protected override Expression VisitLambda<T>(Expression<T> e)
        {
            ParameterExpression[] parameters =
                e.Parameters
                 .Select(Visit)
                 .Cast<ParameterExpression>()
                 .ToArray();

            Expression body = Visit(e.Body);

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

            return Expression.Lambda(funcType, body, e.Name, e.TailCall, parameters);
        }

        /// <inheritdoc />
        protected override Expression VisitMember(MemberExpression e)
        {
            Expression member = Visit(e.Expression);

            if (member == null || e.Member.MemberType != MemberTypes.Property)
                return base.VisitMember(e);

            Type updatedType = _knownTypes.GetOrUpdate(((PropertyInfo) e.Member).PropertyType);

            if (member.Type.GetProperty(e.Member.Name, updatedType) is PropertyInfo p)
                return Expression.Property(member, p);

            return Expression.Default(updatedType);
        }

        /// <inheritdoc />
        protected override Expression VisitMemberInit(MemberInitExpression e)
        {
            MemberBinding[] bindings =
                e.Bindings
                 .Where(x => x.Member.DeclaringType != null)
                 .Where(x => _knownTypes.GetOrUpdate(x.Member.DeclaringType).GetRuntimeProperty(x.Member.Name) != null)
                 .ToArray();

            // TODO: handle MemberBindingType.ListBinding and MemberBindingType.MemberBinding.
            if (bindings.Any(x => x.BindingType != MemberBindingType.Assignment))
                return base.VisitMemberInit(e);

            (string Name, Expression Expression)[] assignments =
                bindings.Cast<MemberAssignment>()
                        .Select(x => (x.Member.Name, Expression: Visit(x.Expression)))
                        .Where(x => !IsDefaultValue(x.Expression))
                        .ToArray();

            Type newType = TypeDefinition.GetOrAdd(assignments.Select(x => (x.Name, x.Expression.Type)).ToArray());

            _knownTypes.Register(e.Type, newType);

            return Expression.MemberInit(
                Expression.New(newType),
                assignments.Select(x => Expression.Bind(newType.GetRuntimeProperty(x.Name), x.Expression)));
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

            Type[] sourceTypeParameters = e.Method.GetGenericArguments();
            Type[] typeParameters = genericMethod.GetGenericArguments();
            ParameterInfo[] parameters = genericMethod.GetParameters();

            for (int i = 0; i < typeParameters.Length; i++)
            {
                Type typeParameter = typeParameters[i];
                for (int j = 0; j < parameters.Length; j++)
                {
                    Type t = InferGenericArgument(typeParameter, parameters[j].ParameterType, arguments[j].Type);

                    if (t == null)
                        continue;

                    typeParameters[i] = _knownTypes.GetOrUpdate(t);
                    break;
                }

                if (typeParameters[i].IsGenericParameter)
                    typeParameters[i] = sourceTypeParameters[i];
            }

            MethodInfo method = genericMethod.MakeGenericMethod(typeParameters);

            // TODO: find a better way to handle these cases.
            if (method.Name == "Average" && e.Arguments.Count == 2 && ((LambdaExpression) arguments[1]).Body is DefaultExpression average)
                return average;

            if (method.Name == "Sum" && e.Arguments.Count == 2 && ((LambdaExpression) arguments[1]).Body is DefaultExpression sum)
                return sum;

            return Expression.Call(instance, method, arguments);
        }

        /// <inheritdoc />
        protected override Expression VisitNew(NewExpression e)
        {
            if (e.Arguments.Count == 0 || e.Members == null)
                return base.VisitNew(e);

            (string Name, Expression Expression)[] arguments =
                e.Arguments
                 .Zip(e.Members, (a, m) => (m.Name, Expression: Visit(a)))
                 .Where(x => !IsDefaultValue(x.Expression))
                 .ToArray();

            Type newType =
                TypeDefinition.GetOrAdd(arguments.Select(x => (x.Name, x.Expression.Type)).ToArray());

            _knownTypes.Register(e.Type, newType);

            ConstructorInfo ctor =
                newType.GetConstructor(arguments.Select(x => x.Expression.Type).ToArray());

            if (ctor == null)
                throw new MissingMemberException($"Constructor not found for {newType.FullName} with {arguments.Length} parameters.");

            return Expression.New(
                ctor,
                arguments.Select(x => x.Expression),
                newType.GetRuntimeProperties().Where(x => arguments.Any(y => y.Name == x.Name)));
        }

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression e)
        {
            if (_knownTypes.TryGetValue(e.Type, out ParameterExpression p))
                return p;

            Type type = _knownTypes.GetOrUpdate(e.Type);

            if (e.Type != type)
                _knownTypes.Register(e.Type, type);

            return _knownTypes.TryGetValue(e.Type, out p) ? p : base.VisitParameter(e);
        }

        /// <summary>
        /// Attempts to infer the concrete type used in place of a specific type parameter,
        /// such as the concrete type of `TKey` in <see cref="IGrouping{TKey,TValue}"/>.
        /// </summary>
        /// <param name="typeParameter">The type parameter being inferred.</param>
        /// <param name="parameter">The parameter of the generic method.</param>
        /// <param name="argument">The argument to the current method.</param>
        /// <returns>
        /// The inferred type or null.
        /// </returns>
        [Pure]
        [CanBeNull]
        static Type InferGenericArgument([NotNull] Type typeParameter, [NotNull] Type parameter, [NotNull] Type argument)
        {
            // N.B. consider T[] == IEnumerable<T>
            if ((!parameter.IsGenericType || !argument.IsGenericType) && !argument.IsArray)
                return null;

            Type[] parameterTypeArguments =
                parameter.GetGenericArguments();

            Type interfaceType = NegotiateMutualInterface(parameter, argument);

            Type[] argumentTypeArguments =
                interfaceType.IsGenericType
                    ? interfaceType.GetGenericArguments()
                    : interfaceType.IsArray
                        ? new[] { interfaceType.GetElementType() }
                        : new Type[0];

            for (int i = 0; i < parameterTypeArguments.Length; i++)
            {
                if (typeParameter.IsAssignableFrom(parameterTypeArguments[i]))
                    return argumentTypeArguments[i];

                if (InferGenericArgument(typeParameter, parameterTypeArguments[i], argumentTypeArguments[i]) is Type result)
                    return result;
            }

            return null;
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

        /// <summary>
        /// Tests if the expression is a constant default value.
        /// </summary>
        /// <param name="e">The expression to test.</param>
        /// <returns>
        /// True if the expression is a constant default value; otherwise, false.
        /// </returns>
        [Pure]
        static bool IsDefaultValue([NotNull] Expression e)
        {
            if (e.NodeType == ExpressionType.Default)
                return true;

            if (!(e is ConstantExpression c))
                return false;

            switch (c.Value)
            {
                case null:      return true;
                case char v:    return v == default;
                case bool v:    return v == default;
                case byte v:    return v == default;
                case sbyte v:   return v == default;
                case decimal v: return v == default;
                case double v:  return v == default;
                case float v:   return v == default;
                case int v:     return v == default;
                case uint v:    return v == default;
                case long v:    return v == default;
                case ulong v:   return v == default;
                case short v:   return v == default;
                case ushort v:  return v == default;
                default:        return false;
            }
        }
    }
}