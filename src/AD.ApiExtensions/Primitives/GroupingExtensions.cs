using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Primitives
{
    /// <summary>
    /// Provides extension methods for <see cref="Grouping{TKey,TValue}"/> and <see cref="GroupingValues{TKey,TValue}"/>.
    /// </summary>
    [PublicAPI]
    public static class GroupingExtensions
    {
        /// <summary>
        /// Casts the elements of an <see cref="IEnumerable{T}"/> as <see cref="Grouping{TKey,TValue}"/>.  
        /// </summary>
        /// <param name="source">
        /// The source collection.
        /// </param>
        /// <typeparam name="TKey">
        /// The type of the keys.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of the values.
        /// </typeparam>
        /// <returns>
        /// The resulting enumerable collection.
        /// </returns>
        public static IEnumerable<Grouping<TKey, TValue>> AsGrouping<TKey, TValue>([NotNull] this IEnumerable<IGrouping<TKey, TValue>> source)
        {
            return source.Cast<Grouping<TKey, TValue>>();
        }
    }
}