using System;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
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
        /// <param name="type">
        /// The type to search for a <see cref="ConstructorInfo"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ConstructorInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentException"/>
        [Pure]
        [NotNull]
        public static ConstructorInfo GetEmptyConstructor([NotNull] this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!(type.GetConstructor(Type.EmptyTypes) is ConstructorInfo constructor))
            {
                throw new ArgumentException($"The type '{type.Name}' does not have an empty constructor");
            }

            return constructor;
        }

        /// <summary>
        /// Gets the specified property or throws an exception if it is not available.
        /// </summary>
        /// <param name="type">
        /// The type to search for a <see cref="ConstructorInfo"/>.
        /// </param>
        /// <param name="memberInfo"></param>
        /// <param name="bindingFlags"></param>
        /// <returns>
        /// The <see cref="ConstructorInfo"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        [Pure]
        [NotNull]
        public static PropertyInfo GetPropertyInfo([NotNull] this Type type, [NotNull] MemberInfo memberInfo, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            if (!(type.GetProperty(memberInfo.Name, bindingFlags) is PropertyInfo propertyInfo))
            {
                throw new ArgumentException($"The type '{type.Name}' does not have a property named '{memberInfo.Name}' with '{bindingFlags.ToString()}' binding.");
            }

            return propertyInfo;
        }
    }
}