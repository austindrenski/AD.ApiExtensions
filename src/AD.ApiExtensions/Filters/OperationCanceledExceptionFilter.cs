using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.ApiExtensions.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Handles an <see cref="OperationCanceledException"/>.
    /// </summary>
    [PublicAPI]
    public class OperationCanceledExceptionFilter : ExceptionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnException([NotNull] ExceptionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!(context.Exception is OperationCanceledException))
            {
                return;
            }

            context.ExceptionHandled = true;
            context.Result = new BadRequestResult();
        }
    }
}