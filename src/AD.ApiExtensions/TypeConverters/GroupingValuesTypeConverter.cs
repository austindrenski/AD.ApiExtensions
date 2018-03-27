using System;
using System.ComponentModel;
using System.Globalization;
using AD.ApiExtensions.Primitives;
using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.TypeConverters
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a type converter that creates <see cref="GroupingValues{TKey,TValue}" /> instances from valid objects.
    /// </summary>
    [PublicAPI]
    public class GroupingValuesTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return
                sourceType == typeof(string) ||
                sourceType == typeof(StringValues) ||
                base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case string s:
                {
                    return GroupingValues<string, string>.Parse(s);
                }
                case StringValues s:
                {
                    return GroupingValues<string, string>.Parse(s);
                }
                default:
                {
                    return base.ConvertFrom(context, culture, value);
                }
            }
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return
                destinationType == typeof(string)
                    ? value.ToString()
                    : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}