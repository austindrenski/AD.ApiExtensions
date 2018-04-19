using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace AD.ApiExtensions.Http
{
    /// <inheritdoc cref="IMiddleware"/>
    /// <summary>
    /// Handles a <typeparamref name="TException"/> by returning an HTTP status code response.
    /// </summary>
    [PublicAPI]
    public class ExceptionMap<TException> : IMiddleware where TException : Exception
    {
        /// <summary>
        /// The HTTP status code.
        /// </summary>
        public int HttpStatusCode { get; }

        /// <summary>
        /// Constructs a <see cref="ExceptionMap{TException}"/> with the specified order value.
        /// </summary>
        /// <param name="httpStatusCode">
        /// The HTTP status code.
        /// </param>
        public ExceptionMap(int httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
        }

        /// <inheritdoc />
        [NotNull]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
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

                context.Response.Clear();
                context.Response.StatusCode = HttpStatusCode;
            }
        }
    }
}