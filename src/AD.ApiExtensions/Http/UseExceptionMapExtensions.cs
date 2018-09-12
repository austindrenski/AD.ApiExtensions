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
    public static class UseExceptionMapExtensions
    {
        /// <summary>
        /// Maps an exception of <typeparamref name="TException"/> to an HTTP status code response.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IApplicationBuilder" /> instance.
        /// </param>
        /// <param name="httpStatusCode">
        /// The HTTP status code to which the exception is mapped.
        /// </param>
        /// <typeparam name="TException">
        /// The type of exception being mapped.
        /// </typeparam>
        /// <returns>
        /// The <see cref="IApplicationBuilder"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static IApplicationBuilder UseExceptionMap<TException>([NotNull] this IApplicationBuilder builder, int httpStatusCode) where TException : Exception
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Use(next => (context => new ExceptionMap<TException>(httpStatusCode).InvokeAsync(context, next)));
        }
    }
}