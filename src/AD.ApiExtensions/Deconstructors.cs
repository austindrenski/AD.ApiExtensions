using System.Collections.Generic;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Provides tuple-syntax deconstructors.
    /// </summary>
    [PublicAPI]
    public static class Deconstructors
    {
        /// <summary>
        /// Deconstructs an item into a value tuple.
        /// </summary>
        /// <param name="keyValuePair">
        /// The item to deconstruct.
        /// </param>
        /// <param name="key">
        /// The out key.
        /// </param>
        /// <param name="value">
        /// The out value.
        /// </param>
        /// <typeparam name="TKey">
        /// The type of the key.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of the value.
        /// </typeparam>
        [Pure]
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
        {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }
    }
}