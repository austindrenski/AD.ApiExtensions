using System;
using AD.ApiExtensions.Formatters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AD.ApiExtensions.Mvc
{
    /// <summary>
    /// Provides extensions to configure <see cref="MvcOptions"/>.
    /// </summary>
    [PublicAPI]
    public static class OutputFormatterConfigurations
    {
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
    }
}