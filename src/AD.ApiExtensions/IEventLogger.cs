using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace AD.ApiExtensions
{
    /// <inheritdoc />
    [PublicAPI]
    public interface IEventLogger : ILogger { }
}