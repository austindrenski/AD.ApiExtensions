using System;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AD.ApiExtensions.Logging
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a logger that includes date and time information.
    /// </summary>
    [PublicAPI]
    public class DateTimeEventLogger<T> : IEventLogger<T>
    {
        private readonly Guid _sessionId = Guid.NewGuid();

        [NotNull] private readonly ILogger _logger;

        [NotNull] private readonly Func<ILoggingContext> _context;

        [NotNull] private readonly Func<Guid, string, LogEntry> _logEntryConstructor;

        /// <summary>
        /// Constructs a new logger that wraps an existing implementation.
        /// </summary>
        /// <param name="logger">
        /// The logger to be wrapped.
        /// </param>
        /// <param name="options">
        ///
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public DateTimeEventLogger([NotNull] ILogger logger, [NotNull] IOptions<DateTimeEventLoggerOptions> options)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _logger = logger;
            _context = options.Value.Context ?? throw new ArgumentException(nameof(options.Value.Context));
            _logEntryConstructor = options.Value.LogEntryConstructor ?? throw new ArgumentNullException(nameof(options.Value.LogEntryConstructor));
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new logger instance from the factory.
        /// </summary>
        /// <param name="loggerFactory">
        /// The factory used to construct the internal logger instance.
        /// </param>
        /// <param name="options">
        ///
        /// </param>
        /// <returns>
        /// A new logger instance.
        /// </returns>
        public DateTimeEventLogger([NotNull] ILoggerFactory loggerFactory, IOptions<DateTimeEventLoggerOptions> options)
            : this(new Logger<T>(loggerFactory), options)
        {
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Func<TState, Exception, string> clean = Clean(formatter);

            if (string.IsNullOrWhiteSpace(clean(state, exception)))
            {
                return;
            }

            _logger.Log(logLevel, eventId, state, exception, (s, ex) => $"[{DateTime.Now:s}]: {clean(s, ex)}");

            if (!(state is ValueTuple<string, IPAddress> tuple))
            {
                return;
            }

            string json = $"{{ \"source\": \"Default\", \"message\": \"{Json(clean, state, exception)}\", \"ipAddress\": \"{tuple.Item2}\" }}";

            // TODO: Make this robust to connection failures with a logging queue.
            try
            {
                using (ILoggingContext context = _context())
                {
                    context.AddLogEntry(_logEntryConstructor(_sessionId, json));
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private Func<TState, Exception, string> Clean<TState>(Func<TState, Exception, string> formatter)
        {
            return (s, ex) => formatter(s, ex).Trim('\r', '\n');
        }

        private string Json<TState>(Func<TState, Exception, string> formatter, TState state, Exception exception)
        {
            return formatter(state, exception).Replace("\"", "\\\"");
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }
    }
}