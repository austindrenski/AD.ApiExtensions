using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions
{
    // TODO: document StringValuesExtensions
    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public static class StringValuesExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="values">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        public static StringValues ToLower(this StringValues values)
        {
            return values.Select(x => x.ToLower()).ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="values">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        public static StringValues ToUpper(this StringValues values)
        {
            return values.Select(x => x.ToUpper()).ToArray();
        }
    }
}