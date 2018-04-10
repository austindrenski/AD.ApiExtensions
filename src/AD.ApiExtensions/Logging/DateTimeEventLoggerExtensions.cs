using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="builder">
        /// The <see cref="ILoggingBuilder"/> to which the <see cref="DateTimeEventLogger{T}"/> is added.
        /// </param>
        /// <param name="implementationFactory">
        /// The factory that creates the service.
        /// </param>
        /// <typeparam name="T">
        /// A type implementing <see cref="ILogContext"/>.
        /// </typeparam>
        [NotNull]
        public static ILoggingBuilder AddDateTimeEvent<T>([NotNull] this ILoggingBuilder builder, [NotNull] Func<IServiceProvider, T> implementationFactory) where T : class, ILogContext
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (implementationFactory is null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            builder.Services
                   .AddScoped<ILogContext>(implementationFactory)
                   .AddScoped(typeof(IEventLogger<>), typeof(DateTimeEventLogger<>));

            return builder;
        }

        /// <summary>
        /// Adds a date-time logger for a scoped service of <see cref="IEventLogger{T}" />.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ILoggingBuilder"/> to which the <see cref="DateTimeEventLogger{T}"/> is added.
        /// </param>
        /// <typeparam name="T">
        /// A type implementing <see cref="ILogContext"/>.
        /// </typeparam>
        [NotNull]
        public static ILoggingBuilder AddDateTimeEvent<T>([NotNull] this ILoggingBuilder builder) where T : class, ILogContext
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services
                   .AddScoped<ILogContext, T>()
                   .AddScoped(typeof(IEventLogger<>), typeof(DateTimeEventLogger<>));

            return builder;
        }
    }
}