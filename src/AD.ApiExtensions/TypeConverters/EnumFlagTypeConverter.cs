using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.TypeConverters
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a type converter that creates flag enums from <see cref="String"/>, <see cref="StringValues"/>, and <see cref="StringSegment"/> objects.
    /// </summary>
    [PublicAPI]
    public sealed class EnumFlagTypeConverter<T> : TypeConverter where T : struct, IComparable, IFormattable, IConvertible
    {
        /// <inheritdoc />
        /// <exception cref="InvalidEnumArgumentException" />
        public EnumFlagTypeConverter()
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidEnumArgumentException(typeof(T).Name);
            }
        }

        /// <inheritdoc />
        [Pure]
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return
                sourceType == typeof(string) ||
                sourceType == typeof(StringValues) ||
                sourceType == typeof(StringSegment) ||
                base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        [Pure]
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case string s:
                {
                    return Parse(s.Split(','));
                }
                case StringValues s:
                {
                    return Parse(s.SelectMany(x => x.Split(',')));
                }
                case StringSegment s:
                {
                    return Parse(s.ToString().Split(','));
                }
                default:
                {
                    return base.ConvertFrom(context, culture, value);
                }
            }
        }

        /// <summary>
        /// Parses each item in the collection as <typeparamref name="T"/> and performs bitwise-or combination.
        /// </summary>
        /// <param name="values">
        /// The items to parse.
        /// </param>
        /// <returns>
        /// The bitwise-or combination of the valid enums parsed from the values.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        private static object Parse([NotNull] IEnumerable<string> values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return
                values.Select(x => Enum.TryParse(x.KebabCaseToCamelCase(), true, out T result) ? Convert.ToUInt64(result) : default)
                      .Aggregate(
                          default(ulong),
                          (current, next) => current | next,
                          result => Enum.ToObject(typeof(T), result));
        }
    }
}