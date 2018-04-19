using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.ApiExtensions.Mvc
{
    /// <inheritdoc cref="Attribute"/>
    /// <inheritdoc cref="IAsyncActionFilter"/>
    /// <inheritdoc cref="IApiResponseMetadataProvider"/>
    /// <summary>
    /// Represents a filter that checks if the model is valid after model binding is complete.
    /// Produces a <see cref="StatusCodes.Status400BadRequest"/> if the model is not valid.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ModelValidationAttribute : Attribute, IAsyncActionFilter, IApiResponseMetadataProvider
    {
        private readonly bool _objectResult;

        /// <inheritdoc />
        public Type Type { get; } = typeof(ModelStateDictionary);

        /// <inheritdoc />
        public int StatusCode { get; } = StatusCodes.Status400BadRequest;

        /// <inheritdoc />
        /// <summary>
        /// Constructs a <see cref="ModelValidationAttribute"/>.
        /// </summary>
        /// <param name="objectResult">
        /// True to return a <see cref="BadRequestObjectResult"/>; otherwise false to return a <see cref="BadRequestResult"/>.
        /// </param>
        public ModelValidationAttribute(bool objectResult = true)
        {
            _objectResult = objectResult;
        }

        /// <inheritdoc />
        public void SetContentTypes([NotNull] MediaTypeCollection contentTypes)
        {
            if (contentTypes is null)
            {
                throw new ArgumentNullException(nameof(contentTypes));
            }
        }

        /// <inheritdoc />
        public async Task OnActionExecutionAsync([NotNull] ActionExecutingContext context, [NotNull] ActionExecutionDelegate next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (context.ModelState.IsValid)
            {
                await next();
            }
            else
            {
                context.Result = _objectResult ? new BadRequestObjectResult(context.ModelState) : (IActionResult) new BadRequestResult();
            }
        }
    }
}