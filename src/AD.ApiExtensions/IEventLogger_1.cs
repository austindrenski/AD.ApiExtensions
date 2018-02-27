using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace AD.ApiExtensions
{
    /// <inheritdoc cref="ILogger{T}"/>
    /// <summary>
    /// Represents a logger that records event information.
    /// </summary>
    /// <typeparam name="T">
    /// The type of state being recorded.
    /// </typeparam>
    [PublicAPI]
    public interface IEventLogger<out T> : IEventLogger, ILogger<T> { }
}