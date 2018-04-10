using System;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

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

        [NotNull] private readonly ILogContext _context;

        /// <summary>
        /// Constructs a new logger that wraps an existing implementation.
        /// </summary>
        /// <param name="logger">
        /// The logger to be wrapped.
        /// </param>
        /// <param name="context">
        /// The <see cref="ILogContext"/> to which log entries are saved.
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public DateTimeEventLogger([NotNull] ILogger logger, [NotNull] ILogContext context)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _logger = logger;
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new logger instance from the factory.
        /// </summary>
        /// <param name="loggerFactory">
        /// The factory used to construct the internal logger instance.
        /// </param>
        /// <param name="context">
        /// The <see cref="ILogContext"/> to which log entries are saved.
        /// </param>
        /// <returns>
        /// A new logger instance.
        /// </returns>
        public DateTimeEventLogger([NotNull] ILoggerFactory loggerFactory, [NotNull] ILogContext context)
            : this(new Logger<T>(loggerFactory), context)
        {
        }

        /// <inheritdoc />
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
                _context.AddLogEntry(_sessionId, json);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <inheritdoc />
        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        private Func<TState, Exception, string> Clean<TState>(Func<TState, Exception, string> formatter)
        {
            return (s, ex) => formatter(s, ex).Trim('\r', '\n');
        }

        private string Json<TState>(Func<TState, Exception, string> formatter, TState state, Exception exception)
        {
            return formatter(state, exception).Replace("\"", "\\\"");
        }
    }
}