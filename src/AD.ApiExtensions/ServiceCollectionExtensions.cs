using System;
using System.Linq;
using AD.ApiExtensions.Mvc;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Provides extensions to configure <see cref="IServiceCollection"/>.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="ExceptionProvider{TException}"/> to the <see cref="IServiceCollection"/>
        /// and an <see cref="ExceptionFilter{TException}"/> to <see cref="P:MvcOptions.Filters"/>.
        /// </summary>
        /// <param name="services">The services to modify.</param>
        /// <param name="httpMethod">The HTTP method to support.</param>
        /// <param name="httpStatusCode">The HTTP status code for the <see cref="ExceptionFilter{TException}"/>.</param>
        /// <param name="providerOrder">The value that determines provider execution order.</param>
        /// <param name="filterOrder">The value that determines filter execution order.</param>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <returns>
        /// The modified <see cref="IServiceCollection"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="services"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="httpMethod"/></exception>
        [NotNull]
        public static IServiceCollection AddExceptionProvider<TException>(
            [NotNull] this IServiceCollection services,
            [NotNull] string httpMethod,
            int httpStatusCode,
            int providerOrder = int.MinValue,
            int filterOrder = default)
            where TException : Exception
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (httpMethod == null)
                throw new ArgumentNullException(nameof(httpMethod));

            ExceptionProvider<TException> provider =
                new ExceptionProvider<TException>(httpMethod, httpStatusCode, providerOrder, filterOrder);

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiDescriptionProvider>(provider));

            return services.Configure<MvcOptions>(x => x.Filters.Add(provider.ExceptionFilter));
        }
    }
}