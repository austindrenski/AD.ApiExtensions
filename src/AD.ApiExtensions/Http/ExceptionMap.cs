using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AD.ApiExtensions.Http
{
    /// <inheritdoc cref="IMiddleware"/>
    /// <summary>
    /// Handles a <typeparamref name="TException"/> by returning an HTTP status code response.
    /// </summary>
    [PublicAPI]
    public class ExceptionMap<TException> : IMiddleware, IApiResponseMetadataProvider where TException : Exception
    {
        /// <inheritdoc />
        public Type Type { get; } = typeof(void);

        /// <inheritdoc />
        public int StatusCode { get; }

        /// <summary>
        /// Constructs a <see cref="ExceptionMap{TException}"/> with the specified order value.
        /// </summary>
        /// <param name="httpStatusCode">
        /// The HTTP status code.
        /// </param>
        public ExceptionMap(int httpStatusCode)
        {
            StatusCode = httpStatusCode;
        }

        /// <inheritdoc />
        public void SetContentTypes([NotNull] MediaTypeCollection contentTypes)
        {
            if (contentTypes == null)
            {
                throw new ArgumentNullException(nameof(contentTypes));
            }
        }

        /// <inheritdoc />
        [NotNull]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            try
            {
                await next(context);
            }
            catch (TException)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.StatusCode = StatusCode;
            }
        }
    }
}