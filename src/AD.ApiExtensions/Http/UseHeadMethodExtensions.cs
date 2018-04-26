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
    public static class HeadMethodExtensions
    {
        /// <summary>
        /// Configures the <see cref="IApplicationBuilder"/> to handle HTTP HEAD requests.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IApplicationBuilder"/> to modify.
        /// </param>
        /// <param name="sendContentHeaders">
        /// True if HTTP HEAD responses should transmit content headers; otherwise, false.
        /// </param>
        /// <returns>
        /// The modified <see cref="IApplicationBuilder"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public static IApplicationBuilder UseHeadMethod([NotNull] this IApplicationBuilder builder, bool sendContentHeaders = true)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Use((context, next) => new HeadMethod(sendContentHeaders).InvokeAsync(context, x => next()));
        }
    }
}