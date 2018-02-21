using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Specifies that one of the parameters must be present in the query string.
    /// </summary>
    [PublicAPI]
    public class RequiredAnyAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The name of the required query parameters.
        /// </summary>
        private readonly StringValues _names;

        /// <summary>
        /// Specifies that one of the parameters must be present in the query string.
        /// </summary>
        /// <param name="names">The parameter names of which one is required.</param>
        public RequiredAnyAttribute(params string[] names)
        {
            _names = names;
        }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (_names.Count == 0)
            {
                return;
            }

            KeyValuePair<string, StringValues>[] parameters =
                actionContext.HttpContext
                             .Request
                             .Query
                             .Where(x => _names.Contains(x.Key))
                             .ToArray();

            bool check0 =
                parameters.Any(x => _names.Contains(x.Key));

            bool check1 =
                parameters.All(x => x.Value
                                     .SelectMany(y => y.Split(','))
                                     .All(y => !string.IsNullOrEmpty(y)
                                               && !string.IsNullOrWhiteSpace(y)));

            if (check0 && check1)
            {
                return;
            }

            actionContext.Result =
                new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    ContentType = "text/plain",
                    Content =
                        _names.Count == 1
                            ? $"Parameter '{_names.Single()}' is required."
                            : $"Parameters [{string.Join(", ", _names)}] are required."
                };
        }
    }
}