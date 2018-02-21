using System;
using System.Linq;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Logging
{
    /// <summary>
    /// Provides configuration options for <see cref="DateTimeEventLogger{T}"/>.
    /// </summary>
    [PublicAPI]
    public class DateTimeEventLoggerOptions
    {
        /// <summary>
        /// The database context used for logging.
        /// </summary>
        [CanBeNull]
        public Func<ILoggingContext> Context { get; set; }

        /// <summary>
        /// Constructs a log entry.
        /// </summary>
        [CanBeNull]
        public Func<Guid, string, ILogEntry> LogEntryConstructor { get; set; }
    }
}