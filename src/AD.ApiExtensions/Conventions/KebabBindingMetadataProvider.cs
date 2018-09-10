using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AD.ApiExtensions.Conventions
{
    /// <inheritdoc cref="IBindingMetadataProvider" />
    /// <inheritdoc cref="IDisplayMetadataProvider" />
    [PublicAPI]
    public class KebabBindingMetadataProvider : IBindingMetadataProvider, IDisplayMetadataProvider
    {
        /// <inheritdoc />
        public void CreateBindingMetadata([NotNull] BindingMetadataProviderContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (IsController(context.Key.ContainerType) || !context.Attributes.OfType<FromQueryAttribute>().Any())
                return;

            if (context.BindingMetadata.BinderModelName is null)
                context.BindingMetadata.BinderModelName = context.Key.Name?.ConvertToKebabCase();
        }

        /// <inheritdoc />
        public void CreateDisplayMetadata([NotNull] DisplayMetadataProviderContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (IsController(context.Key.ContainerType) || !context.Attributes.OfType<FromQueryAttribute>().Any())
                return;

            if (context.DisplayMetadata.DisplayName is null)
                context.DisplayMetadata.DisplayName = () => context.Key.Name?.ConvertToKebabCase();
        }

        /// <summary>
        /// Tests if the type has or inherits a <see cref="ControllerAttribute"/>.
        /// </summary>
        /// <param name="type">
        /// The type to test.
        /// </param>
        /// <returns>
        /// True if the type has or inherits a <see cref="ControllerAttribute"/>; otherwise, false.
        /// </returns>
        /// <remarks>
        /// ASP.NET Core 2.0 introduces controller members to this middleware.
        /// Altering controller members interferes with dependency injection.
        /// </remarks>
        static bool IsController([CanBeNull] Type type)
            => type != null && type.IsDefined(typeof(ControllerAttribute), true);
    }
}