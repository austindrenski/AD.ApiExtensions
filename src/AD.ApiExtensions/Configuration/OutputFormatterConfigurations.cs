using System;
using AD.ApiExtensions.OutputFormatters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace AD.ApiExtensions.Configuration
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
        /// <param name="options">
        /// The options to modify.
        /// </param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static MvcOptions AddXmlOutputFormatter([NotNull] this MvcOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
            options.OutputFormatters.Add(new XmlOutputFormatter());
            return options;
        }

        /// <summary>
        /// Adds an <see cref="DelimitedOutputFormatter"/> to the <see cref="MvcOptions"/>.
        /// </summary>
        /// <param name="options">
        /// The options to modify.
        /// </param>
        /// <returns>
        /// The <see cref="MvcOptions"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static MvcOptions AddDelimitedOutputFormatter([NotNull] this MvcOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
            options.FormatterMappings.SetMediaTypeMappingForFormat("psv", "text/psv");
            options.FormatterMappings.SetMediaTypeMappingForFormat("tsv", "text/tab-separated-values");
            options.OutputFormatters.Add(new DelimitedOutputFormatter());

            return options;
        }
    }
}