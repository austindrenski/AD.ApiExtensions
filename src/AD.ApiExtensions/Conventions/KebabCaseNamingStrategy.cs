using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;

namespace AD.ApiExtensions.Conventions
{
    /// <inheritdoc />
    /// <summary>
    /// A kebab case naming strategy.
    /// </summary>
    [PublicAPI]
    public sealed class KebabCaseNamingStrategy : NamingStrategy
    {
        /// <inheritdoc />
        [Pure]
        [CanBeNull]
        protected override string ResolvePropertyName([CanBeNull] string name) => name.ConvertToKebabCase();
    }
}