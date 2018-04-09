using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;

namespace AD.ApiExtensions.Conventions
{
    /// <inheritdoc />
    /// <summary>
    /// Used by <see cref="Newtonsoft.Json.JsonSerializer" /> to resolve a <see cref="JsonContract" /> for a given type.
    /// </summary>
    [PublicAPI]
    public class KebabContractResolver : DefaultContractResolver
    {
        /// <inheritdoc />
        public KebabContractResolver()
        {
            NamingStrategy = new KebabCaseNamingStrategy();
        }
    }
}