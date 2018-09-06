using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AD.ApiExtensions.Mvc
{
    /// <inheritdoc cref="IAsyncActionFilter"/>
    /// <inheritdoc cref="IExceptionFilter"/>
    /// <inheritdoc cref="IOrderedFilter"/>
    /// <summary>
    /// Handles a <typeparamref name="TException"/> by returning a <see cref="StatusCodeResult"/>.
    /// </summary>
    [PublicAPI]
    public class ExceptionFilter<TException>
        : IAsyncExceptionFilter,
          IExceptionFilter,
          IOrderedFilter,
          IApiResponseMetadataProvider,
          IEquatable<ApiResponseType> where TException : Exception
    {
        /// <inheritdoc />
        public int Order { get; }

        /// <inheritdoc />
        public int StatusCode { get; }

        /// <inheritdoc />
        public virtual Type Type { get; }

        /// <summary>
        /// Constructs a <see cref="ExceptionFilter{TException}"/> with the specified HTTP status code.
        /// </summary>
        /// <param name="httpStatusCode">
        /// The HTTP status code of the result.
        /// </param>
        /// <param name="order">
        /// The value determining the execution order of the filter.
        /// </param>
        public ExceptionFilter(int httpStatusCode, int order)
        {
            StatusCode = httpStatusCode;
            Order = order;
            Type = typeof(void);
        }

        /// <inheritdoc />
        public virtual void SetContentTypes([NotNull] MediaTypeCollection contentTypes)
        {
            if (contentTypes is null)
                throw new ArgumentNullException(nameof(contentTypes));
        }

        /// <inheritdoc />
        public virtual void OnException([NotNull] ExceptionContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (!(context.Exception is TException))
                return;

            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(StatusCode);
        }

        /// <inheritdoc />
        [NotNull]
        public virtual Task OnExceptionAsync([NotNull] ExceptionContext context)
        {
            if (context is null)

                throw new ArgumentNullException(nameof(context));

            OnException(context);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(ApiResponseType other) => other != null && Type == other.Type && StatusCode == other.StatusCode;
    }
}