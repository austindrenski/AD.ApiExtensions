using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AD.ApiExtensions.Mvc
{
    /// <inheritdoc />
    /// <summary>
    /// An <see cref="T:Microsoft.AspNetCore.Mvc.ActionResult" /> that produces a <see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict" /> response.
    /// </summary>
    [PublicAPI]
    public class ConflictResult : StatusCodeResult
    {
        /// <inheritdoc />
        /// <summary>
        /// Constructs a <see cref="T:AD.ApiExtensions.Http.ConflictResult" /> that produces a <see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict" /> response.
        /// </summary>
        public ConflictResult() : base(StatusCodes.Status409Conflict) { }
    }
}