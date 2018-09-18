using System.Linq;
using System.Linq.Expressions;
using AD.ApiExtensions.Types;
using JetBrains.Annotations;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [UsedImplicitly]
    public class TypeCacheTest
    {
        [Fact]
        public void GroupByUpdated()
        {
            var cache = new TypeCache();
            var values = new[] { new { A = "a", B = "b", C = "c" } };

            var type = values.First().GetType();
            var updatedType = TypeDefinition.GetOrAdd(new[] { ("A", typeof(string)), ("B", typeof(string)) });
            cache.Register(type, updatedType);

            var group = values.AsQueryable().GroupBy(x => x);
            var method = (MethodCallExpression) group.Expression;
            var updatedMethod = cache.GetOrUpdate(method.Method);
            var updatedArguments = method.Arguments.Select(cache.GetOrAddParameter).ToArray();

            var call = Expression.Call(updatedMethod, updatedArguments);

            var result = group.Provider.Execute(call);

            Assert.Equal(updatedType, result.GetType());
        }
    }
}