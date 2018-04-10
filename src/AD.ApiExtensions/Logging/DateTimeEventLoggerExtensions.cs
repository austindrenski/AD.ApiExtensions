using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AD.ApiExtensions.Logging
{
    /// <summary>
    /// Represents a logger that includes date and time information.
    /// </summary>
    [PublicAPI]
    public static class DateTimeEventLoggerExtensions
    {
        /// <summary>
        /// Adds a date-time logger for a scoped service of <see cref="IEventLogger{T}" />.
        /// </summary>
        /// <param name="services">
        /// The service collection to override.
        /// </param>
        /// <typeparam name="T">
        /// The implementation type of the <see cref="ILogContext"/>.
        /// </typeparam>
        [NotNull]
        public static IServiceCollection AddDateTimeEventLogger<T>(this IServiceCollection services) where T : class, ILogContext
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ILoggerFactory, LoggerFactory>();
            services.TryAddScoped<ILogContext, T>();
            services.TryAddScoped(typeof(IEventLogger<>), typeof(DateTimeEventLogger<>));
            return services;
        }
    }
}