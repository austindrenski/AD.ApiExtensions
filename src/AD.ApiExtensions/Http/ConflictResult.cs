using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AD.ApiExtensions.Http
{
    /// <summary>
    /// An <see cref="ActionResult" /> that produces a <see cref="StatusCodes.Status409Conflict" /> response.
    /// </summary>
    [PublicAPI]
    public class ConflictResult : StatusCodeResult
    {
        /// <summary>
        /// Constructs a <see cref="ConflictResult" /> that produces a <see cref="StatusCodes.Status409Conflict" /> response.
        /// </summary>
        public ConflictResult() : base(StatusCodes.Status409Conflict) { }
    }
}