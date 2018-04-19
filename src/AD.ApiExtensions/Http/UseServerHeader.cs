using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace AD.ApiExtensions.Http
{
    /// <summary>
    /// Provides extensions to add middleware to an <see cref="IApplicationBuilder"/>.
    /// </summary>
    [PublicAPI]
    public static class UseServerHeaderExtensions
    {
        /// <summary>
        /// Sets the 'Server' header.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IApplicationBuilder" /> instance.
        /// </param>
        /// <param name="server">
        /// The server name to use. Defaults to <see cref="string.Empty"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IApplicationBuilder"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static IApplicationBuilder UseServerHeader([NotNull] this IApplicationBuilder builder, [CanBeNull] string server = default)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return
                builder.Use(
                    async (context, next) =>
                    {
                        context.Response.Headers.Add("Server", server ?? string.Empty);
                        await next();
                    });
        }
    }
}