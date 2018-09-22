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
        [NotNull] readonly Dictionary<Type, ParameterExpression> _types = new Dictionary<Type, ParameterExpression>();

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

            if (_types.TryGetValue(type, out ParameterExpression parameterExpression))
                return parameterExpression.Type;

            if (!type.IsGenericType)
                return type;

            Type recurse = Recurse(type);

            return _types.TryGetValue(recurse, out parameterExpression) ? parameterExpression.Type : recurse;

            Type Recurse(Type t) => t.GetGenericTypeDefinition().MakeGenericType(t.GenericTypeArguments.Select(GetOrUpdate).ToArray());
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="type">The type of the value to get.</param>
        /// <param name="result">If this method returns true, contains the parameter associated with the specified type.</param>
        /// <returns>
        /// True if the <see cref="TypeCache"/> contains an element with the specified type; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        [Pure]
        [ContractAnnotation("type:null => false, result:null; => result:notnull")]
        public bool TryGetValue([CanBeNull] Type type, out ParameterExpression result)
        {
            result = null;

            return type != null && _types.TryGetValue(type, out result);
        }

        /// <summary>
        /// Updates the initial type and adds it to the type and parameter cache, provided
        /// that the initial type does not already exist in the cache. If the type already
        /// exists in the cache and <paramref name="update"/> is true, then the parameter
        /// is replaced, provided the existing parameter and the replacement type differ.
        /// </summary>
        /// <param name="type">The initial type.</param>
        /// <param name="result">The result type.</param>
        /// <param name="update">True if existing keys should be updated; otherwise, false.</param>
        /// <returns>
        /// True if any type was registered or updated; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="result"/></exception>
        public bool Register([NotNull] Type type, [NotNull] Type result, bool update = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            // The key already exists.
            if (_types.ContainsKey(type))
            {
                if (update && type != result)
                    _types[type] = Expression.Parameter(result, result.Name);

                return update;
            }

            // The key already exists for an updated type.
            if (GetOrUpdate(type) is Type t && _types.ContainsKey(t))
            {
                if (update && t != result)
                    _types[t] = Expression.Parameter(result, result.Name);

                return update;
            }

            // The key does not yet exist.
            _types.Add(type, Expression.Parameter(result, result.Name));

            return true;
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
                => _types.Select(x => x.Value.Type)
                         .SelectMany(x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                         .Select(y => y.Name)
                         .Contains(name);
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