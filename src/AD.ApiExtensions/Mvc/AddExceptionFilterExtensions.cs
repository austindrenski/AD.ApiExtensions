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

            options.Filters.Add(new BaseExceptionFilter<TException, TResult>(order));
            return options;
        }
    }
}