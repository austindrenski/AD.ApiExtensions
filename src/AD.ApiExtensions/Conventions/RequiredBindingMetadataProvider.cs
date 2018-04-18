using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AD.ApiExtensions.Conventions
{
    /// <inheritdoc />
    /// <summary>
    /// Modifies the behavior of <see cref="T:System.ComponentModel.DataAnnotations.RequiredAttribute" /> instances by setting <see cref="P:BindingMetadata.IsBindingRequired" /> to true.
    /// </summary>
    [PublicAPI]
    public sealed class RequiredBindingMetadataProvider : IBindingMetadataProvider
    {
        /// <inheritdoc />
        public void CreateBindingMetadata([NotNull] BindingMetadataProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PropertyAttributes?.OfType<RequiredAttribute>().Any() ?? false)
            {
                context.BindingMetadata.IsBindingRequired = true;
            }
        }
    }
}