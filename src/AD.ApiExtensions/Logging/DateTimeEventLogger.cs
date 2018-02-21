using System;
using System.Linq;
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
    public static class DateTimeLogger
    {
        /// <summary>
        /// Adds a date-time logger for a singleton service of <see cref="T:Microsoft.Extensions.Logging.ILogger{T}" />.
        /// </summary>
        /// <param name="services">
        /// The service collection to override.
        /// </param>
        /// <param name="configure">
        /// 
        /// </param>
        public static IServiceCollection AddDateTimeEventLogger(this IServiceCollection services, Action<DateTimeEventLoggerOptions> configure)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
            services.Replace(ServiceDescriptor.Singleton(typeof(IEventLogger<>), typeof(DateTimeEventLogger<>)));
            services.Configure(configure);
            return services;
        }
    }
}