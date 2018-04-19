using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.ApiExtensions.Mvc
{
    /// <summary>
    /// Provides extensions to configure <see cref="MvcOptions"/>.
    /// </summary>
    [PublicAPI]
    public static class AddModelValidationExtensions
    {
        /// <summary>
        /// Adds an <see cref="IExceptionFilter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">
        /// The options to modify.
        /// </param>
        /// <param name="objectResult">
        /// True to return a <see cref="BadRequestObjectResult"/>; otherwise false to return a <see cref="BadRequestResult"/>.
        /// </param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static MvcOptions AddModelValidation([NotNull] this MvcOptions options, bool objectResult = true)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Filters.Add(new ModelValidationAttribute(objectResult));
            return options;
        }
    }
}