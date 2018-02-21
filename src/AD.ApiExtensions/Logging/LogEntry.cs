using System;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Logging
{
    /// <inheritdoc />
    [PublicAPI]
    public class LogEntry : ILogEntry
    {
        /// <inheritdoc />
        public DateTime DateTime { get; set; }

        /// <inheritdoc />
        public string Event { get; set; }

        /// <inheritdoc />
        public Guid SessionId { get; set; }

        /// <inheritdoc />
        public Guid EventId { get; set; }

        /// <summary>
        /// Constructs an entry with the current date and time.
        /// </summary>
        public LogEntry()
        {
            EventId = Guid.NewGuid();
            DateTime = DateTime.Now;
        }

        /// <inheritdoc />
        /// <param name="sessionId">
        /// The unique identifier for the session.
        /// </param>
        /// <param name="json">
        /// The JSON event message.
        /// </param>
        public LogEntry(Guid sessionId, [NotNull] string json) : this()
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            SessionId = sessionId;
            Event = json;
        }
    }
}