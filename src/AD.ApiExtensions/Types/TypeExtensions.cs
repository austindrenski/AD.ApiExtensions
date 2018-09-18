using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Types
{
    /// <summary>
    /// Provides extensions to access type information.
    /// </summary>
    [PublicAPI]
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the empty constructor of the specified type, or throws an exception if it is not available.
        /// </summary>
        /// <param name="type">The type to search for a <see cref="ConstructorInfo"/>.</param>
        /// <returns>
        /// The <see cref="ConstructorInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentException">'{type.FullName}' does not have an empty constructor.</exception>
        [Pure]
        [NotNull]
        public static ConstructorInfo GetEmptyConstructor([NotNull] this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetConstructor(Type.EmptyTypes) ?? throw new ArgumentException($"'{type.FullName}' does not have an empty constructor.");
        }
    }
}