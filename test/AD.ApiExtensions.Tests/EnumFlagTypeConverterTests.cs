using System;
using AD.ApiExtensions.TypeConverters;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [PublicAPI]
    public static class EnumFlagTypeConverterTests
    {
        [Flags]
        public enum Colors
        {
            None = 0,
            Red = 1 << 0,
            Blue = 1 << 1,
            Yellow = 1 << 2,
        }

        [Theory]
        [InlineData("1", Colors.None)]
        [InlineData("Green", Colors.None)]
        [InlineData("Red", Colors.Red)]
        [InlineData("blue", Colors.Blue)]
        [InlineData("yelloW", Colors.Yellow)]
        [InlineData("red,blue,yellow", Colors.Red | Colors.Blue | Colors.Yellow)]
        public static void Test0([NotNull] string value, Colors expected)
        {
            EnumFlagTypeConverter<Colors> converter = new EnumFlagTypeConverter<Colors>();

            Assert.True(converter.CanConvertFrom(value.GetType()));
            Assert.Equal(expected, converter.ConvertFrom(value));
        }
    }
}