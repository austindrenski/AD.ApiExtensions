using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AD.ApiExtensions.Mvc
{
    /// <summary>
    /// Provides extensions to add an <see cref="ExceptionProvider{TException}"/> to an <see cref="IServiceCollection"/>.
    /// </summary>
    [PublicAPI]
    public static class AddExceptionProviderExtensions
    {
        /// <summary>
        /// Adds an <see cref="ExceptionProvider{TException}"/> to the <see cref="IServiceCollection"/>
        /// and an <see cref="ExceptionFilter{TException,TResult}"/> to <see cref="MvcOptions.Filters"/>.
        /// </summary>
        /// <param name="services">
        /// The services to modify.
        /// </param>
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
        /// <typeparam name="TException"></typeparam>
        /// <returns>
        /// The modified <see cref="IServiceCollection"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static IServiceCollection AddExceptionProvider<TException>([NotNull] this IServiceCollection services, [NotNull] string httpMethod, int httpStatusCode, int providerOrder = int.MinValue, int filterOrder = default) where TException : Exception
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (httpMethod is null)
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }

            ExceptionProvider<TException> provider =
                new ExceptionProvider<TException>(httpMethod, httpStatusCode, providerOrder, filterOrder);

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiDescriptionProvider>(provider));

            return services.Configure<MvcOptions>(x => x.Filters.Add(provider.ExceptionFilter));
        }
    }
}