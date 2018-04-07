using System;
using System.ComponentModel.DataAnnotations;
using AD.ApiExtensions.Attributes;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [PublicAPI]
    public static class EnumBindRequiredAttributeTests
    {
        [Flags]
        public enum Colors
        {
            None = 0,
            Red = 1 << 0,
            Blue = 1 << 1,
            Yellow = 1 << 2,
        }

        [PublicAPI]
        private class Example
        {
            [EnumBindRequired]
            public Colors Colors { get; set; }

            public void Validate() => Validator.ValidateObject(this, new ValidationContext(this), true);
        }

        [Theory]
        [InlineData(Colors.None)]
        [InlineData(Colors.Red)]
        [InlineData(Colors.Blue)]
        [InlineData(Colors.Yellow)]
        [InlineData(Colors.Red | Colors.Blue | Colors.Yellow)]
        public static void Test0(Colors value)
        {
            Example example = new Example { Colors = value };

            Exception exception = Record.Exception(() => example.Validate());

            if (value is Colors.None)
            {
                Assert.NotNull(exception);
            }
            else
            {
                Assert.Null(exception);
            }
        }
    }
}