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
    /// Specifies that a parameter must be present in the query string.
    /// </summary>
    [PublicAPI]
    public class RequiredCountAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The name of the required query parameter.
        /// </summary>
        private readonly StringValues _name;

        /// <summary>
        /// The number of values required for this parameter.
        /// </summary>
        private readonly int _count;

        /// <summary>
        /// Specifies the parameters that must occur in the query string.
        /// </summary>
        /// <param name="name">The parameter name that is required.</param>
        /// <param name="count"></param>
        public RequiredCountAttribute(string name, int count)
        {
            _name = name;
            _count = count;
        }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            KeyValuePair<string, StringValues>[] parameters =
                actionContext.HttpContext
                             .Request
                             .Query
                             .Where(x => _name.Contains(x.Key))
                             .ToArray();

            bool check0 =
                _name.All(x => parameters.Select(y => y.Key).Contains(x));

            bool check1 =
                parameters.All(x => x.Value
                                     .SelectMany(y => y.Split(','))
                                     .All(y => !string.IsNullOrEmpty(y)
                                            && !string.IsNullOrWhiteSpace(y)));

            bool check2 =
                parameters.All(x => x.Value
                                     .SelectMany(y => y.Split(','))
                                     .Count() == _count);

            if (check0 && check1 && check2)
            {
                return;
            }

            actionContext.Result =
                new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    ContentType = "text/plain",
                    Content =
                        _name.Count == 1
                            ? $"Parameter '{_name.Single()}' is required to have {_count} values."
                            : $"Parameters [{string.Join(", ", _name)}] are required to have {_count} values."
                };
        }
    }
}