using System.ComponentModel.DataAnnotations;
using AD.ApiExtensions.Attributes;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [UsedImplicitly]
    public class PatternValidationAttributeTests
    {
        [PublicAPI]
        class Example
        {
            [PatternValidation("^[A-z]{3}$")]
            public string Name { get; set; }

            public void Validate() => Validator.ValidateObject(this, new ValidationContext(this), true);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("ab")]
        [InlineData("ab0")]
        [InlineData("abc0")]
        public static void Test0(string name)
        {
            Example example = new Example { Name = name };

            Assert.Throws<ValidationException>(() => example.Validate());
        }
    }
}