using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.ApiExtensions.Mvc
{
    /// <summary>
    /// Provides extensions to configure <see cref="MvcOptions"/>.
    /// </summary>
    [PublicAPI]
    public static class AddExceptionFilterExtensions
    {
        /// <summary>
        /// Adds an <see cref="IExceptionFilter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">
        /// The options to modify.
        /// </param>
        /// <param name="httpStautsCode">
        /// The HTTP status code of the result.
        /// </param>
        /// <param name="order">
        /// The order value for determining the order of execution of filters.
        /// </param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static MvcOptions AddExceptionFilter<TException>([NotNull] this MvcOptions options, int httpStautsCode, int order = default)
            where TException : Exception
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Filters.Add(new ExceptionFilter<TException>(httpStautsCode, order));
            return options;
        }

        /// <summary>
        /// Adds an <see cref="IExceptionFilter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">
        /// The options to modify.
        /// </param>
        /// <param name="order">
        /// The order value for determining the order of execution of filters.
        /// </param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static MvcOptions AddExceptionFilter<TException, TResult>([NotNull] this MvcOptions options, int order = default)
            where TException : Exception
            where TResult : StatusCodeResult, new()
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Filters.Add(new ExceptionFilter<TException, TResult>(order));
            return options;
        }
    }
}