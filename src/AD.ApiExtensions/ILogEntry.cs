using System;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public interface ILogEntry
    {
        /// <summary>
        ///
        /// </summary>
        DateTime DateTime { get; }

        /// <summary>
        ///
        /// </summary>
        string Event { get; }

        /// <summary>
        ///
        /// </summary>
        Guid SessionId { get; set; }

        /// <summary>
        ///
        /// </summary>
        Guid EventId { get; set; }
    }
}