using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// that the initial type does not already exist in the cache.
        /// </summary>
        /// <param name="type">The initial type.</param>
        /// <param name="result">The result type.</param>
        /// <returns>
        /// True if any type was registered or updated; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="result"/></exception>
        public void Register([NotNull] Type type, [NotNull] Type result)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (_types.ContainsKey(type))
            {
                if (_types[type].Type != result)
                    _types[type] = Expression.Parameter(result, result.Name);

                return;
            }

            _types.Add(type, Expression.Parameter(result, result.Name));
        }
    }
}