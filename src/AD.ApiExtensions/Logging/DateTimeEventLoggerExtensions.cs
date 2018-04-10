﻿using System;
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
        /// Adds a date-time logger for a scoped service of <see cref="IEventLogger" />.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ILoggingBuilder"/> to which the <see cref="DateTimeEventLogger"/> is added.
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
                   .AddScoped<IEventLogger, DateTimeEventLogger>();

            return builder;
        }

        /// <summary>
        /// Adds a date-time logger for a scoped service of <see cref="IEventLogger" />.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ILoggingBuilder"/> to which the <see cref="DateTimeEventLogger"/> is added.
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

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DateTimeEventLoggerProvider>());

            builder.Services
                   .AddScoped<ILogContext, T>()
                   .AddScoped<IEventLogger, DateTimeEventLogger>();

            return builder;
        }
    }
}