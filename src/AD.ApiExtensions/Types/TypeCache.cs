using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Types
{
    /// <summary>
    /// Represents a cache of types and updates.
    /// </summary>
    [PublicAPI]
    public class TypeCache
    {
        /// <summary>
        /// Caches anonymous types that were encountered and updated.
        /// </summary>
        [NotNull] readonly Dictionary<Type, Type> _types = new Dictionary<Type, Type>();

        /// <summary>
        /// Caches anonymous types that were encountered and modified.
        /// </summary>
        [NotNull] readonly Dictionary<Type, ParameterExpression> _parameters = new Dictionary<Type, ParameterExpression>();

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2"></see> contains the specified key.
        /// </summary>
        /// <param name="type">The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</param>
        /// <returns>
        /// True if the <see cref="TypeCache"/> contains an element of the specified type; otherwise, false.
        /// </returns>
        [Pure]
        [ContractAnnotation("type:null => false")]
        public bool ContainsKey([CanBeNull] Type type) => type != null && _types.ContainsKey(type);

        /// <summary>
        /// Returns an updated type or the initial type if no update is required.
        /// </summary>
        /// <param name="type">The type to update.</param>
        /// <returns>
        /// An updated type or the initial type if no update is required.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        [Pure]
        [NotNull]
        public Type GetOrUpdate([NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _types.TryGetValue(type, out Type result) ? result : type;
        }

        /// <summary>
        /// Returns an updated method or the initial method if no update is required.
        /// </summary>
        /// <param name="method">The method to update.</param>
        /// <returns>
        /// An updated method or the initial method if no update is required.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="method"/></exception>
        [Pure]
        [NotNull]
        public MethodInfo GetOrUpdate([NotNull] MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return
                method.IsGenericMethod
                    ? method.GetGenericMethodDefinition()
                            .MakeGenericMethod(
                                 method.GetGenericArguments()
                                       .Select(GetOrUpdate)
                                       .ToArray())
                    : method;
        }

        /// <summary>
        /// Returns an updated <see cref="ParameterExpression"/> or the initial expression if no update
        /// is required, or if the initial expression is not a <see cref="ParameterExpression"/>.
        /// </summary>
        /// <param name="expression">The expression to update.</param>
        /// <returns>
        /// An updated <see cref="ParameterExpression"/> or the initial expression if no update is required.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/></exception>
        [Pure]
        [NotNull]
        public Expression GetOrAddParameter([NotNull] Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType != ExpressionType.Parameter)
                return expression;

            if (TryGetParameter(expression.Type, out ParameterExpression parameter))
                return parameter;

            Register(expression.Type, expression.Type);

            return _parameters.TryGetValue(expression.Type, out parameter) ? parameter : expression;
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="type">The type of the value to get.</param>
        /// <param name="result">If this method returns true, contains the parameter associated with the specified type.</param>
        /// <returns>
        /// True if the <see cref="TypeCache"/> contains an element with the specified type; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        [Pure]
        [ContractAnnotation("type:null => false, result:null")]
        public bool TryGetParameter([NotNull] Type type, [NotNull] out ParameterExpression result)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _parameters.TryGetValue(type, out result);
        }

        /// <summary>
        /// Updates the initial type and adds it to the type and parameter cache, provided
        /// that the initial type is a constructed generic type and that the type does not
        /// already exist in the cache.
        /// </summary>
        /// <param name="type">The initial type.</param>
        /// <param name="result">The result type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="result"/></exception>
        public void Register([NotNull] Type type, [NotNull] Type result)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (!type.IsConstructedGenericType)
                return;

            if (_types.ContainsKey(type))
                return;

            Type updatedType = RecurseType(result);

            if (!_parameters.TryGetValue(updatedType, out ParameterExpression parameter))
                parameter = Expression.Parameter(updatedType, $"param_{updatedType.Name}");

            _types.Add(type, parameter.Type);
            _parameters.Add(type, parameter);
        }

        /// <summary>
        /// Tests if it is provable that an assignment results in a default initialization.
        /// </summary>
        /// <param name="memberInfo">The target of assignment.</param>
        /// <param name="expression">The expression for assignment.</param>
        /// <param name="unavailable">The property names that were explicitly removed by the time of testing.</param>
        /// <returns>
        /// True if it is provable that the assignment results in a default initialization; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="unavailable"/></exception>
        [Pure]
        public bool IsLogicallyDefault(
            [NotNull] MemberInfo memberInfo,
            [NotNull] Expression expression,
            [NotNull] IDictionary<string, MemberInfo> unavailable)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (unavailable == null)
                throw new ArgumentNullException(nameof(unavailable));

            if (expression is ConstantExpression constant)
                return IsDefault(constant.Value);

            if (_types.Count == 0 || IsUpdatedMemberName(memberInfo.Name))
                return false;

            return unavailable.TryGetValue(memberInfo.Name, out MemberInfo m) && m.GetType() == memberInfo.GetType();

            bool IsUpdatedMemberName(string name)
                => _types.Select(x => x.Value)
                         .SelectMany(x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                         .Select(y => y.Name)
                         .Contains(name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        ///
        /// </returns>
        [Pure]
        [NotNull]
        Type RecurseType([NotNull] Type type)
        {
            if (!type.IsConstructedGenericType)
                return type;

            return
                GetOrUpdate(
                    type.GetGenericTypeDefinition()
                        .MakeGenericType(
                             type.GenericTypeArguments
                                 .Select(RecurseType)
                                 .ToArray()));
        }

        /// <summary>
        /// Tests if the item is recognized as the default value of the underlying type.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// True if the item is recognized as a default value; otherwise false.
        /// </returns>
        [Pure]
        static bool IsDefault([CanBeNull] object value)
        {
            switch (value)
            {
                case null:      return true;
                case char v:    return v == '\0';
                case bool v:    return v == false;
                case byte v:    return v == 0;
                case sbyte v:   return v == 0;
                case decimal v: return v == 0;
                case double v:  return v == 0;
                case float v:   return v == 0;
                case int v:     return v == 0;
                case uint v:    return v == 0;
                case long v:    return v == 0;
                case ulong v:   return v == 0;
                case short v:   return v == 0;
                case ushort v:  return v == 0;
                default:
                    return false;
            }
        }
    }
}