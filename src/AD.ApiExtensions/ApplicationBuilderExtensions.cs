using System;
using AD.ApiExtensions.Http;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Provides extensions to add middleware to an <see cref="IApplicationBuilder"/>.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Maps an exception of <typeparamref name="TException"/> to an HTTP status code response.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder" /> instance.</param>
        /// <param name="httpStatusCode">The HTTP status code to which the exception is mapped.</param>
        /// <typeparam name="TException">The type of exception being mapped.</typeparam>
        /// <returns>
        /// The <see cref="IApplicationBuilder"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/></exception>
        [NotNull]
        public static IApplicationBuilder UseExceptionMap<TException>(
            [NotNull] this IApplicationBuilder builder, int httpStatusCode)
            where TException : Exception
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Use(next => context => new ExceptionMap<TException>(httpStatusCode).InvokeAsync(context, next));
        }

        /// <summary>
        /// Configures the <see cref="IApplicationBuilder"/> to handle HTTP HEAD requests.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> to modify.</param>
        /// <param name="sendContentHeaders">True if HTTP HEAD responses should transmit content headers; otherwise, false.</param>
        /// <returns>
        /// The modified <see cref="IApplicationBuilder"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/></exception>
        [NotNull]
        public static IApplicationBuilder UseHeadMethod([NotNull] this IApplicationBuilder builder, bool sendContentHeaders = true)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.Use(next => ctx => new HeadMethod(sendContentHeaders).InvokeAsync(ctx, next));
        }

        /// <summary>
        /// Sets the 'Server' header.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder" /> instance.</param>
        /// <param name="send">True if a server header should be sent; otherwise, false.</param>
        /// <returns>
        /// The mutated <see cref="IApplicationBuilder"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder"/></exception>
        [NotNull]
        public static IApplicationBuilder UseServerHeader([NotNull] this IApplicationBuilder builder, bool send = false)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return
                builder.Use(
                    (ctx, next) =>
                    {
                        if (!send && !ctx.Response.HasStarted)
                            ctx.Response.Headers.Add("Server", string.Empty);

                        return next();
                    });
        }
    }
}