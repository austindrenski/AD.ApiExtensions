using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.ApiExtensions.Mvc
{
    /// <inheritdoc />
    /// <summary>
    /// Provides a <see cref="ExceptionFilter{TException}"/> to methods with a given HTTP method.
    /// </summary>
    [PublicAPI]
    public class ExceptionProvider<TException> : IApiDescriptionProvider where TException : Exception
    {
        /// <inheritdoc />
        public int Order { get; }

        /// <summary>
        /// The HTTP method described by this provider.
        /// </summary>
        public string HttpMethod { get; }

        /// <summary>
        /// The HTTP status code produced by this provider.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// The exception filter.
        /// </summary>
        public ExceptionFilter<TException> ExceptionFilter { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="providerOrder"></param>
        /// <param name="filterOrder"></param>
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
        public void OnProvidersExecuted([NotNull] ApiDescriptionProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (ApiDescription description in context.Results)
            {
                if (description.HttpMethod != HttpMethod)
                {
                    continue;
                }

                FilterDescriptor descriptor = new FilterDescriptor(ExceptionFilter, ExceptionFilter.Order);

                description.ActionDescriptor.FilterDescriptors.Add(descriptor);
            }
        }
    }
}