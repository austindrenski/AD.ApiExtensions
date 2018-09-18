using System;
using System.Linq;
using AD.ApiExtensions.Formatters;
using AD.ApiExtensions.Mvc;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.ApiExtensions
{
    /// <summary>
    /// Provides extensions to configure <see cref="MvcOptions"/>.
    /// </summary>
    [PublicAPI]
    public static class MvcOptionsExtensions
    {
        #region Exceptions

        /// <summary>
        /// Adds an <see cref="IExceptionFilter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">The options to modify.</param>
        /// <param name="httpStatusCode">The HTTP status code of the result.</param>
        /// <param name="order">The order value for determining the order of execution of filters.</param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/></exception>
        [NotNull]
        public static MvcOptions AddExceptionFilter<TException>(
            [NotNull] this MvcOptions options,
            int httpStatusCode,
            int order = default)
            where TException : Exception
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Filters.Add(new ExceptionFilter<TException>(httpStatusCode, order));
            return options;
        }

        /// <summary>
        /// Adds an <see cref="IExceptionFilter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">The options to modify.</param>
        /// <param name="order">The order value for determining the order of execution of filters.</param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/></exception>
        [NotNull]
        public static MvcOptions AddExceptionFilter<TException, TResult>(
            [NotNull] this MvcOptions options,
            int order = default)
            where TException : Exception
            where TResult : StatusCodeResult, new()
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Filters.Add(new ExceptionFilter<TException, TResult>(order));
            return options;
        }

        #endregion

        #region OutputFormatters

        /// <summary>
        /// Adds an <see cref="XmlOutputFormatter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">The options to modify.</param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/></exception>
        [NotNull]
        public static MvcOptions AddXmlOutputFormatter([NotNull] this MvcOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            XmlOutputFormatter formatter = new XmlOutputFormatter();
            formatter.Add("application/xml");

            options.OutputFormatters.Add(formatter);
            options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");

            return options;
        }

        /// <summary>
        /// Adds an <see cref="DelimitedOutputFormatter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">The options to modify.</param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/></exception>
        [NotNull]
        public static MvcOptions AddDelimitedOutputFormatter([NotNull] this MvcOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DelimitedOutputFormatter formatter = new DelimitedOutputFormatter();
            formatter.Add("text/csv", ',');
            formatter.Add("text/psv", '|');
            formatter.Add("text/tab-separated-values", '\t');

            options.OutputFormatters.Add(formatter);
            options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
            options.FormatterMappings.SetMediaTypeMappingForFormat("psv", "text/psv");
            options.FormatterMappings.SetMediaTypeMappingForFormat("tsv", "text/tab-separated-values");

            return options;
        }

        /// <summary>
        /// Adds an <see cref="HtmlOutputFormatter{T}"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">The options to modify.</param>
        /// <param name="viewPath">The view path.</param>
        /// <param name="modelFactory">The model factory.</param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="viewPath"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="modelFactory"/></exception>
        [NotNull]
        public static MvcOptions AddHtmlOutputFormatter<T>(
            [NotNull] this MvcOptions options,
            [NotNull] [PathReference] string viewPath,
            [NotNull] Func<HttpContext, object, T> modelFactory)
        {
            if (viewPath == null)
                throw new ArgumentNullException(nameof(viewPath));

            if (modelFactory == null)
                throw new ArgumentNullException(nameof(modelFactory));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            HtmlOutputFormatter<T> formatter = new HtmlOutputFormatter<T>(viewPath, modelFactory);
            formatter.Add("text/html");
            formatter.Add("text/xhtml");

            options.OutputFormatters.Add(formatter);
            options.FormatterMappings.SetMediaTypeMappingForFormat("html", "text/html");
            options.FormatterMappings.SetMediaTypeMappingForFormat("xhtml", "text/xhtml");

            return options;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Adds an <see cref="ModelValidationAttribute"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">The options to modify.</param>
        /// <param name="objectResult">True to return a <see cref="BadRequestObjectResult"/>; otherwise false to return a <see cref="BadRequestResult"/>.</param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/></exception>
        [NotNull]
        public static MvcOptions AddModelValidationFilter([NotNull] this MvcOptions options, bool objectResult = true)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Filters.Add(new ModelValidationAttribute(objectResult));
            return options;
        }

        #endregion
    }
}