using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace AD.ApiExtensions.Mvc
{
    /// <inheritdoc />
    /// <summary>
    /// Provides a <see cref="ExceptionFilter{TException}"/> to methods with a given HTTP method.
    /// </summary>
    [PublicAPI]
    public class ExceptionProvider<TException> : IApiDescriptionProvider where TException : Exception
    {
        /// <summary>
        /// The exception filter.
        /// </summary>
        [NotNull]
        public ExceptionFilter<TException> ExceptionFilter { get; }

        /// <summary>
        /// The HTTP method supported by this provider.
        /// </summary>
        [NotNull]
        public string HttpMethod { get; }

        /// <inheritdoc />
        public int Order { get; }

        /// <summary>
        /// The HTTP status code produced by this provider.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Constructs an <see cref="ExceptionProvider{TException}"/>.
        /// </summary>
        /// <param name="httpMethod">
        /// The HTTP method to support.
        /// </param>
        /// <param name="httpStatusCode">
        /// The HTTP status code for the <see cref="ExceptionFilter{TException}"/>.
        /// </param>
        /// <param name="providerOrder">
        /// The value that determines provider execution order.
        /// </param>
        /// <param name="filterOrder">
        /// The value that determines filter execution order.
        /// </param>
        public ExceptionProvider([NotNull] string httpMethod, int httpStatusCode, int providerOrder, int filterOrder)
        {
            if (httpMethod is null)
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }

            HttpMethod = httpMethod;
            StatusCode = httpStatusCode;
            Order = providerOrder;
            ExceptionFilter = new ExceptionFilter<TException>(StatusCode, filterOrder);
        }

        /// <inheritdoc />
        public void OnProvidersExecuting([NotNull] ApiDescriptionProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Removes the <see cref="ExceptionFilter"/> from unqualified candidates.
        /// </remarks>
        public void OnProvidersExecuted([NotNull] ApiDescriptionProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (ApiDescription description in context.Results)
            {
                if (description.HttpMethod == HttpMethod)
                {
                    continue;
                }

                ApiResponseType[] removals =
                    description.SupportedResponseTypes
                               .Where(x => ExceptionFilter.Equals(x))
                               .ToArray();

                foreach (ApiResponseType responseType in removals)
                {
                    description.SupportedResponseTypes.Remove(responseType);
                }
            }
        }
    }
}