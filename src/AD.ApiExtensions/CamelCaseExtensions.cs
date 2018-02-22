using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Provides string format extensions.
    /// </summary>
    [PublicAPI]
    public static class CamelCaseExtentions
    {
        /// <summary>
        /// The regular expression to match camel case text.
        /// </summary>
        [NotNull] [ProvidesContext] private static readonly Regex CamelCaseRegex = new Regex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", RegexOptions.Compiled);

        /// <summary>
        /// Applies the regular expression to change camel case style to kebab style.
        /// </summary>
        /// <param name="value">
        /// The value to modify.
        /// </param>
        /// <returns>
        /// The value in kebab style.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public static string CamelCaseToKebabCase([NotNull] this string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return CamelCaseRegex.Replace(value, "-$1").ToLower();
        }

        /// <summary>
        /// Applies the regular expression to change camel case style to path style.
        /// </summary>
        /// <param name="value">
        /// The value to modify.
        /// </param>
        /// <returns>
        /// The value in path style.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public static string CamelCaseToPathCase([NotNull] this string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return CamelCaseRegex.Replace(value, "/$1").ToLower();
        }

        /// <summary>
        /// Applies the regular expression to change camel case style to space style.
        /// </summary>
        /// <param name="value">
        /// The value to modify.
        /// </param>
        /// <returns>
        /// The value in path style.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public static string CamelCaseToSpaceCase([NotNull] this string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return CamelCaseRegex.Replace(value, " $1");
        }

        // TODO: this should be recapitalizing
        /// <summary>
        /// Applies the regular expression to change kebab style to camel case style.
        /// </summary>
        /// <param name="value">
        /// The value to modify.
        /// </param>
        /// <returns>
        /// The value in camel case style.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public static string KebabCaseToCamelCase([NotNull] this string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.Replace("-", null).ToLower();
        }

        /// <summary>
        /// Applies the regular expression to change camel case to snake case.
        /// </summary>
        /// <param name="value">
        /// The value to modify.
        /// </param>
        /// <returns>
        /// The value in camel case style.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public static string CamelCaseToSnakeCase([NotNull] this string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return CamelCaseRegex.Replace(value, "_$1").ToLower();
        }
    }
}