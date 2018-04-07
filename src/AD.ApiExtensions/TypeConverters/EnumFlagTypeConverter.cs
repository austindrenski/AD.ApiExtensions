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
        /// <summary>
        /// The names of the enum members.
        /// </summary>
        private static readonly HashSet<string> Names = new HashSet<string>(Enum.GetNames(typeof(T)));

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
                sourceType == typeof(StringSegment);
        }

        private static readonly char[] Splits = new char[] { ',' };

        /// <inheritdoc />
        [Pure]
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case string s:
                {
                    return Parse(new StringSegment(s).Split(Splits));
                }
                case StringValues s:
                {
                    return Parse(s.SelectMany(x => new StringSegment(x).Split(Splits)));
                }
                case StringSegment s:
                {
                    return Parse(s.Split(Splits));
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
        private static object Parse([NotNull] IEnumerable<StringSegment> values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return
                values.Select(x => x.Value.KebabCaseToCamelCase())
                      .Where(x => Names.Contains(x, StringComparer.OrdinalIgnoreCase))
                      .Select(x => Enum.TryParse(x, true, out T result) ? Convert.ToUInt64(result) : default)
                      .Aggregate(
                          default(ulong),
                          (current, next) => current | next,
                          result => Enum.ToObject(typeof(T), result));
        }
    }
}