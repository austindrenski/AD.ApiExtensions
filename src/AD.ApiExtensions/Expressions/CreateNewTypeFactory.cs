using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    /// <summary>
    /// Extension method to create new types at runtime from an <see cref="IEnumerable{T}"/> of <see cref="PropertyInfo"/>.
    /// </summary>
    [PublicAPI]
    public static class CreateNewTypeFactory
    {
        [NotNull] private static readonly ConcurrentDictionary<int, Type> Types = new ConcurrentDictionary<int, Type>();

        /// <summary>
        /// Creates a new type similar to CLR-generated anonymous types. The type is named with the pattern: f__Anonymous__{GUID}.
        /// Unlike CLR-generated anonymous types, the created type is serializable.
        /// </summary>
        /// <param name="properties">
        /// The property information to include in the new type.
        /// </param>
        /// <returns>
        /// A new type that behaves like a CLR-generated anonymous type.
        /// </returns>
        [Pure]
        [NotNull]
        public static Type CreateNew([NotNull] [ItemNotNull] this IEnumerable<PropertyInfo> properties)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            PropertyInfo[] propertyInfo = properties as PropertyInfo[] ?? properties.ToArray();

            int hashCode =
                propertyInfo.Select(x => x.GetHashCode())
                            .Aggregate(397, (current, next) => unchecked(current ^ (next * 397)));

            return Types.GetOrAdd(hashCode, (Type) new TypeDefinition(propertyInfo));
        }
        
        /// <summary>
        /// Creates a new type similar to CLR-generated anonymous types. The type is named with the pattern: f__Anonymous__{GUID}.
        /// Unlike CLR-generated anonymous types, the created type is serializable.
        /// </summary>
        /// <param name="properties">
        /// The property information to include in the new type.
        /// </param>
        /// <returns>
        /// A new type that behaves like a CLR-generated anonymous type.
        /// </returns>
        [Pure]
        [NotNull]
        public static Type CreateNew([NotNull] this IEnumerable<(string Name, Type Type)> properties)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            (string Name, Type Type)[] propertyInfo = properties as (string Name, Type Type)[] ?? properties.ToArray();

            int hashCode =
                propertyInfo.Select(x => x.GetHashCode())
                            .Aggregate(397, (current, next) => unchecked(current ^ (next * 397)));

            return Types.GetOrAdd(hashCode, (Type) new TypeDefinition(propertyInfo));
        }
    }
}