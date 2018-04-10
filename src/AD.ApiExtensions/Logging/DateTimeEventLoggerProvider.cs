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
        [NotNull] private readonly ILogger _console;
        [NotNull] private readonly ILogContext _context;
        [NotNull] private readonly ConcurrentDictionary<string, DateTimeEventLogger> _loggers;

        /// <summary>
        /// Construct a <see cref="DateTimeEventLoggerProvider"/>.
        /// </summary>
        /// <param name="console">
        /// A console logger to pipe events through.
        /// </param>
        /// <param name="context">
        /// The <see cref="ILogContext"/> to which log entries are saved.
        /// </param>
        public DateTimeEventLoggerProvider([NotNull] ILogger console, [NotNull] ILogContext context)
        {
            if (console is null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _console = console;
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

            return _loggers.GetOrAdd(categoryName, new DateTimeEventLogger(_console, _context));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _context.SaveChanges();
            _context.Dispose();
        }
    }
}