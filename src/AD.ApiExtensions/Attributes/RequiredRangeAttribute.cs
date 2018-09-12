using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.Attributes
{
    /// <summary>
    /// Specifies that a parameter must be present in the query string.
    /// </summary>
    /// <inheritdoc />
    [PublicAPI]
    public class RequiredRangeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The name of the required query parameter.
        /// </summary>
        [NotNull] readonly string _name;

        /// <summary>
        /// The inclusive lower bound for the number of permissible values.
        /// </summary>
        readonly int _from;

        /// <summary>
        /// The inclusive upper bound for the number of permissible values.
        /// </summary>
        readonly int _to;

        /// <summary>
        /// Specifies the parameters that must occur in the query string.
        /// </summary>
        /// <param name="name">The parameter name that is required.</param>
        /// <param name="from">The inclusive lower bound for the number of permissible values.</param>
        /// <param name="to">The inclusive upper bound for the number of permissible values.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        public RequiredRangeAttribute([NotNull] string name, int from, int to)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _name = name;
            _from = from;
            _to = to;
        }

        /// <inheritdoc />
        public override void OnActionExecuting([NotNull] ActionExecutingContext context)
        {
            KeyValuePair<string, StringValues> parameter =
                context.HttpContext
                       .Request
                       .Query
                       .FirstOrDefault(x => string.Equals(_name, x.Key, StringComparison.OrdinalIgnoreCase));

            bool check1 =
                parameter.Value
                         .SelectMany(y => y.Split(','))
                         .All(y => !string.IsNullOrWhiteSpace(y));

            bool check2 =
                parameter.Value
                         .SelectMany(y => y.Split(','))
                         .Count() >= _from;

            bool check3 =
                parameter.Value
                         .SelectMany(y => y.Split(','))
                         .Count() <= _to;

            if (check1 && check2 && check3)
                return;

            context.Result =
                new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    ContentType = "text/plain",
                    Content = $"Parameter '{_name}' is required to have between {_from} and {_to} values."
                };
        }
    }
}