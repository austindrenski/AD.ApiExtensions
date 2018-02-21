using System;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AD.ApiExtensions.Filters
{
    /// <inheritdoc />
    [PublicAPI]
    public class SwaggerOptionalOperationFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply([NotNull] Operation operation, [NotNull] OperationFilterContext context)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (operation.Parameters is default)
            {
                return;
            }

            foreach (IParameter parameter in operation.Parameters)
            {
                parameter.Required = false;
            }
        }
    }
}