using System.Linq;
using AD.ApiExtensions.Primitives;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [PublicAPI]
    public static class GroupingValuesTests
    {
        [Theory]
        [InlineData("a,b,c,d,e,f,g", 1)]
        [InlineData("a,b,c,group_1(d,e,f),g", 2)]
        [InlineData("a,b,c,group_1(d,e,f),group_2(g)", 3)]
        public static void GroupingValues(string value, int groupCount)
        {
            GroupingValues<string, string> groups = GroupingValues<string, string>.Parse(value);

            Assert.Equal(groupCount, groups.Select(x => x.Key).Count());
            Assert.Equal(groupCount, groups.Count());
            Assert.True(groups.Individuals.All(x => !x.Contains(',')));
        }
    }
}