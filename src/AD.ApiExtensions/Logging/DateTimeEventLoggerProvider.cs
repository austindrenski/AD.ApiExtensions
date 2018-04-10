using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace AD.ApiExtensions.Logging
{
    /// <inheritdoc />
    [PublicAPI]
    [ProviderAlias("DateTimeEvent")]
    public class DateTimeEventLoggerProvider : ILoggerProvider
    {
        [NotNull] private readonly ILogContext _context;
        [NotNull] private readonly ConcurrentDictionary<string, DateTimeEventLogger> _loggers;

        /// <summary>
        /// Construct a <see cref="DateTimeEventLoggerProvider"/>.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ILogContext"/> to which log entries are saved.
        /// </param>
        public DateTimeEventLoggerProvider([NotNull] ILogContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
            _loggers = new ConcurrentDictionary<string, DateTimeEventLogger>();
        }

        /// <inheritdoc />
        public ILogger CreateLogger([NotNull] string categoryName)
        {
            if (categoryName is null)
            {
                throw new ArgumentNullException(nameof(categoryName));
            }

            return _loggers.GetOrAdd(categoryName, new DateTimeEventLogger(_context));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _context.SaveChanges();
            _context.Dispose();
        }
    }
}