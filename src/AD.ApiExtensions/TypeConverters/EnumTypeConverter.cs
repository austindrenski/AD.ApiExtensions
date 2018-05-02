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
    /// Represents a type converter that creates enums from <see cref="String"/>, <see cref="StringValues"/>, and <see cref="StringSegment"/> objects.
    /// </summary>
    [PublicAPI]
    public sealed class EnumTypeConverter<TEnum> : TypeConverter where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        /// <summary>
        /// The names of the enum members.
        /// </summary>
        private static readonly HashSet<string> Names = new HashSet<string>(Enum.GetNames(typeof(TEnum)), StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        /// <exception cref="InvalidEnumArgumentException" />
        public EnumTypeConverter()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new InvalidEnumArgumentException($"{typeof(TEnum).Name} is not an enum.");
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
                    return Parse(s);
                }
                case StringValues s:
                {
                    return Parse(s);
                }
                case StringSegment s:
                {
                    return Parse(s.Value);
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Parses the item as <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">
        /// The item to parse.
        /// </param>
        /// <returns>
        /// The enum parsed from the value.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        private static TEnum Parse([NotNull] string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!Names.Contains(value))
            {
                throw new InvalidEnumArgumentException($"The value '{value}' is not a valid enum of '{typeof(TEnum).Name}'.");
            }

            return Enum.TryParse(value, true, out TEnum result) ? result : default;
        }
    }
}