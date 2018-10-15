using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [UsedImplicitly]
    public class ConvertToCaseTest
    {
        [Theory]
        [InlineData("A1", "a1")]
        [InlineData("AB1", "ab1")]
        [InlineData("ABC1", "abc1")]
        [InlineData("Ab1", "ab1")]
        [InlineData("AbC1", "ab_c1")]
        public void SnakeCase(string name, string expected)
        {
            string result = name.ConvertToSnakeCase();

            Assert.Equal(expected, result);
        }
    }
}