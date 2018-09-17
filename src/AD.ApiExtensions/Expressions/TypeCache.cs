using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    // TODO: document TypeCache
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
        ///
        /// </summary>
        [NotNull] readonly ISet<(string Name, Type Type)> _differences = new HashSet<(string Name, Type Type)>();

        /// <summary>
        /// Caches anonymous types that were encountered and modified.
        /// </summary>
        [NotNull] readonly Dictionary<Type, ParameterExpression> _parameters = new Dictionary<Type, ParameterExpression>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        [Pure]
        [ContractAnnotation("type:null => false")]
        public bool ContainsKey([CanBeNull] Type type) => type != null && _types.ContainsKey(type);

        /// <summary>
        ///
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="methodInfo"/></exception>
        [Pure]
        [NotNull]
        public MethodInfo GetOrUpdate([NotNull] MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            return
                !methodInfo.IsGenericMethod
                    ? methodInfo
                    : methodInfo.GetGenericMethodDefinition()
                                .MakeGenericMethod(
                                     methodInfo.GetGenericArguments()
                                               .Select(GetTypeOrInput)
                                               .ToArray());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>
        ///
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
            return _parameters[expression.Type];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="result"></param>
        /// <returns>
        ///
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
        /// Tests if it is provable that an assignment results in a default initialization.
        /// </summary>
        /// <param name="expression">The expression for assignment.</param>
        /// <param name="unavailable">The property names that were explicitly removed by the time of testing.</param>
        /// <param name="memberInfo">The target of assignment.</param>
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

//            // TODO: look into this to replace the current "unavailable" methodology.
//                return _differences.Contains((memberInfo.Name, memberInfo.GetType()));
            return unavailable.ContainsKey(memberInfo.Name) && unavailable[memberInfo.Name].GetType() == memberInfo.GetType();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="result"></param>
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

            Type recurseType = RecurseType(result);

            if (!_parameters.TryGetValue(recurseType, out ParameterExpression parameter))
                parameter = Expression.Parameter(recurseType, $"param_{recurseType.Name}");

            _types.Add(type, parameter.Type);
            _parameters.Add(type, parameter);

//            // TODO: look into this to replace the current "unavailable" methodology.
//            IEnumerable<(string Name, Type PropertyType)> keys =
//                _types.Select(x => x.Key)
//                      .SelectMany(x => x.GetProperties(BindingFlags.FlattenHierarchy).Select(y => (y.Name, y.PropertyType)))
//                      .Distinct();
//
//            IEnumerable<(string Name, Type PropertyType)> values =
//                _types.Select(x => x.Value)
//                      .SelectMany(x => x.GetProperties(BindingFlags.FlattenHierarchy).Select(y => (y.Name, y.PropertyType)))
//                      .Distinct();
//
//            foreach ((string Name, Type PropertyType) d in keys.Except(values))
//            {
//                _differences.Add(d);
//            }
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
        Type GetTypeOrInput([NotNull] Type type) => _types.TryGetValue(type, out Type result) ? result : type;

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
                GetTypeOrInput(
                    type.GetGenericTypeDefinition()
                        .MakeGenericType(
                             type.GenericTypeArguments
                                 .Select(RecurseType)
                                 .ToArray()));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns>
        ///
        /// </returns>
        bool IsUpdatedMemberName([NotNull] string name)
            => _types.Select(x => x.Value)
                     .SelectMany(x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                     .Select(y => y.Name)
                     .Contains(name);

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