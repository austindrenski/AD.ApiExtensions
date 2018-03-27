using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions
{
    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public readonly struct Delimiter
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly Delimiter Parenthetical = new Delimiter(',', '(', ')');

        /// <summary>
        ///
        /// </summary>
        public static readonly Delimiter Quote = new Delimiter(',', '"', '"');

        /// <summary>
        ///
        /// </summary>
        public char Open { get; }

        /// <summary>
        ///
        /// </summary>
        public char Close { get; }

        /// <summary>
        ///
        /// </summary>
        public char Separator { get; }

        ///  <summary>
        ///
        ///  </summary>
        /// <param name="separator"></param>
        /// <param name="open"></param>
        ///  <param name="close"></param>
        public Delimiter(char separator, char open, char close)
        {
            Open = open;
            Close = close;
            Separator = separator;
        }
    }

    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public static class SplitDelimitedExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="value">
        ///
        /// </param>
        /// <param name="delimiter">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        [Pure]
        [NotNull]
        public static IEnumerable<StringSegment> SplitDelimited([NotNull] this string value, Delimiter delimiter)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            StringSegment remainder = value;
            while (remainder != StringSegment.Empty)
            {
                StringSegment result;
                (result, remainder) = NextSegment(remainder, delimiter);
                yield return result;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="segment">
        ///
        /// </param>
        /// <param name="delimiter">
        ///
        /// </param>
        /// <returns></returns>
        [Pure]
        private static (StringSegment Result, StringSegment Remainder) NextSegment(StringSegment segment, Delimiter delimiter)
        {
            for (int i = 0; i < segment.Length; i++)
            {
                switch (segment[i])
                {
                    case char c when c == delimiter.Open:
                    {
                        return GetSubExpression(segment, delimiter);
                    }
                    case char c when c == delimiter.Separator:
                    {
                        return YieldIfValid(segment, i, i + 1);
                    }
                    default:
                    {
                        continue;
                    }
                }
            }

            return (segment, StringSegment.Empty);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="segment">
        ///
        /// </param>
        /// <param name="delimiter">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentException"/>
        [Pure]
        private static (StringSegment Result, StringSegment Remainder) GetSubExpression(StringSegment segment, Delimiter delimiter)
        {
            for (int i = 0; i < segment.Length; i++)
            {
                if (segment[i] == delimiter.Close)
                {
                    return YieldIfValid(segment, i + 1, i + 2);
                }
            }

            throw new ArgumentException($"Unbounded subexpression. The string segment '{segment}' does not contain the closing character: '{delimiter.Close}'.");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="segment">
        ///
        /// </param>
        /// <param name="index">
        ///
        /// </param>
        /// <param name="start"></param>
        /// <returns>
        ///
        ///  </returns>
        [Pure]
        private static (StringSegment Result, StringSegment Remainder) YieldIfValid(StringSegment segment, int index, int start)
        {
            return (segment.Subsegment(0, index), start < segment.Length ? segment.Subsegment(start) : StringSegment.Empty);
        }
    }
}