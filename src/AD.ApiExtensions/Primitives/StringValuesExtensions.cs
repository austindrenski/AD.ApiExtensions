using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.Primitives
{
    /// <summary>
    /// Provides extension methods for <see cref="StringValues"/>.
    /// </summary>
    [PublicAPI]
    public static class StringValuesExtensions
    {
        /// <summary>
        /// Returns a copy of this <see cref="StringValues"/> converted to lowercase.
        /// </summary>
        /// <param name="values">The source values.</param>
        /// <returns>
        /// The lowercase equivalent of the current <see cref="StringValues"/>.
        /// </returns>
        [Pure]
        public static StringValues ToLower(in this StringValues values)
            => values.Select(x => x.ToLower()).ToArray();

        /// <summary>
        /// Returns a copy of this <see cref="StringValues"/> converted to uppercase.
        /// </summary>
        /// <param name="values">The source values.</param>
        /// <returns>
        /// The uppercase equivalent of the current <see cref="StringValues"/>.
        /// </returns>
        [Pure]
        public static StringValues ToUpper(in this StringValues values)
            => values.Select(x => x.ToUpper()).ToArray();
    }
}