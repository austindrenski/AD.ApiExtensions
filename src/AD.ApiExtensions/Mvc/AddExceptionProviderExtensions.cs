using System;
using JetBrains.Annotations;
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
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <param name="httpMethod"></param>
        /// <param name="httpStautsCode"></param>
        /// <param name="providerOrder"></param>
        /// <param name="filterOrder"></param>
        /// <typeparam name="TException"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static IServiceCollection AddExceptionProvider<TException>([NotNull] this IServiceCollection services, [NotNull] string httpMethod, int httpStautsCode, int providerOrder = int.MinValue, int filterOrder = default) where TException : Exception
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (httpMethod is null)
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApiDescriptionProvider, ExceptionProvider<TException>>(
                    x => new ExceptionProvider<TException>(httpMethod, httpStautsCode, providerOrder, filterOrder)));

            return services;
        }
    }
}