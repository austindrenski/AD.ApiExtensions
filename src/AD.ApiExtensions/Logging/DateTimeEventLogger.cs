﻿using System;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace AD.ApiExtensions.Logging
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a logger that includes date and time information.
    /// </summary>
    [PublicAPI]
    public class DateTimeEventLogger : IEventLogger
    {
        private readonly Guid _sessionId;

        [NotNull] private readonly ILogContext _context;

        /// <summary>
        /// Constructs a new logger that wraps an existing implementation.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ILogContext"/> to which log entries are saved.
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public DateTimeEventLogger([NotNull] ILogContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
            _sessionId = Guid.NewGuid();
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Func<TState, Exception, string> clean = Clean(formatter);

            if (string.IsNullOrWhiteSpace(clean(state, exception)))
            {
                return;
            }

            Console.WriteLine($"[{DateTime.Now:s}]: {clean(state, exception)}");

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
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
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