using System;
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
        [NotNull]
        protected override string ResolvePropertyName([NotNull] string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return name.CamelCaseToKebabCase();
        }
    }
}