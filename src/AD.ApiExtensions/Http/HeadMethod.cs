using System;
using System.IO;
using System.Threading.Tasks;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace AD.ApiExtensions.Http
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a middleware component that sets the response body to an <see cref="ObservableNullStream"/> for HTTP HEAD requests.
    /// </summary>
    [PublicAPI]
    public class HeadMethod : IMiddleware
    {
        /// <summary>
        /// True if HTTP HEAD responses should transmit content headers.
        /// </summary>
        private readonly bool _sendContentHeaders;

        /// <summary>
        /// Constructs a <see cref="HeadMethod"/>.
        /// </summary>
        /// <param name="sendContentHeaders">
        /// True if HTTP HEAD responses should transmit content headers.
        /// </param>
        public HeadMethod(bool sendContentHeaders = true)
        {
            _sendContentHeaders = sendContentHeaders;
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

            if (HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.Body = _sendContentHeaders ? new ObservableNullStream() : Stream.Null;
            }

            await next(context);

            if (HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.ContentLength = _sendContentHeaders ? context.Response.Body.Length : default;
            }
        }
    }
}