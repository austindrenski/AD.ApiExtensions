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
    public sealed class EnumFlagTypeConverter<TEnum> : TypeConverter where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        /// <summary>
        /// Characters to split strings into enums.
        /// </summary>
        private static readonly char[] Splits = new char[] { ',' };

        /// <summary>
        /// The names of the enum members.
        /// </summary>
        private static readonly HashSet<string> Names = new HashSet<string>(Enum.GetNames(typeof(TEnum)));

        /// <inheritdoc />
        /// <exception cref="InvalidEnumArgumentException" />
        public EnumFlagTypeConverter()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new InvalidEnumArgumentException($"{typeof(TEnum).Name} is not an enum.");
            }

            if (!Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute)))
            {
                throw new InvalidEnumArgumentException($"{typeof(TEnum).Name} is not declared with {nameof(FlagsAttribute)}.");
            }
        }

        /// <inheritdoc />
        [Pure]
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || sourceType == typeof(StringValues) || sourceType == typeof(StringSegment);
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
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
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Parses each item in the collection as <typeparamref name="TEnum"/> and performs bitwise-or combination.
        /// </summary>
        /// <param name="values">
        /// The items to parse.
        /// </param>
        /// <returns>
        /// The bitwise-or combination of the valid enums parsed from the values.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        private static TEnum Parse([NotNull] IEnumerable<StringSegment> values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return
                values.Select(x => x.Value.KebabCaseToCamelCase())
                      // TODO: verify that the ignore case condition isn't needed.
                      // ReSharper disable once PossibleUnintendedLinearSearchInSet
                      .Where(x => Names.Contains(x) || Names.Contains(x, StringComparer.OrdinalIgnoreCase))
                      .Select(TryParse)
                      .Aggregate(
                          default(ulong),
                          (current, next) => current | next,
                          result => (TEnum) Enum.ToObject(typeof(TEnum), result));
        }

        [Pure]
        private static ulong TryParse([NotNull] string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!Enum.TryParse(value, true, out TEnum result))
            {
                throw new InvalidEnumArgumentException($"The value '{value}' is not a valid enum of '{typeof(TEnum).Name}'.");
            }

            return Convert.ToUInt64(result);
        }
    }
}