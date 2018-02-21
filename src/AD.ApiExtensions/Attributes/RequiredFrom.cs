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
    /// <inheritdoc />
    /// <summary>
    /// Specifies that a parameter must be present in the query string.
    /// </summary>
    [PublicAPI]
    public class RequiredFromAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The name of the query parameter.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The options available for this parameter.
        /// </summary>
        private readonly StringValues _options;

        /// <summary>
        /// Specifies the parameters that must occur in the query string.
        /// </summary>
        /// <param name="name">
        /// The parameter name.
        /// </param>
        /// <param name="options">
        /// The values that this parameter may take.
        /// </param>
        public RequiredFromAttribute(string name, params string[] options)
        {
            _name = name;
            _options = options;
        }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            KeyValuePair<string, StringValues> parameter =
                actionContext.HttpContext
                             .Request
                             .Query
                             .SingleOrDefault(x => _name.Equals(x.Key));

            bool check0 = !parameter.Equals(default(KeyValuePair<string, StringValues>));

            bool check1 =
                parameter.Value
                         .SelectMany(y => y.Split(','))
                         .All(y => _options.Contains(y, StringComparer.OrdinalIgnoreCase));

            if (check0 && check1)
            {
                return;
            }

            actionContext.Result =
                new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    ContentType = "text/plain",
                    Content = $"Parameter '{_name}' is required to be from the set {{ {string.Join(", ", _options)} }}."
                };
        }
    }
}