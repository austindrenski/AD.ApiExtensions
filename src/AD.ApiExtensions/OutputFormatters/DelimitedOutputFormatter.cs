using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace AD.ApiExtensions.OutputFormatters
{
    /// <inheritdoc />
    /// <summary>
    /// Writes an object in delimited format to the output stream.
    /// </summary>
    [PublicAPI]
    public class DelimitedOutputFormatter : IOutputFormatter
    {
        /// <summary>
        /// The collection of supported media types.
        /// </summary>
        [NotNull] private static readonly Dictionary<MediaTypeHeaderValue, char> SupportedMediaTypes;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static DelimitedOutputFormatter()
        {
            SupportedMediaTypes =
                new Dictionary<MediaTypeHeaderValue, char>
                {
                    [MediaTypeHeaderValue.Parse("text/csv")] = ',',
                    [MediaTypeHeaderValue.Parse("text/psv")] = '|',
                    [MediaTypeHeaderValue.Parse("text/tab-separated-values")] = '\t'
                };
        }

        /// <inheritdoc />
        [Pure]
        public bool CanWriteResult([NotNull] OutputFormatterCanWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return
                SupportedMediaTypes.Keys.Contains(MediaTypeHeaderValue.Parse(context.ContentType)) ||
                context.HttpContext.Request.Headers["user-agent"].Any(x => x?.StartsWith("Stata") ?? false);
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            char delimiter = SupportedMediaTypes[MediaTypeHeaderValue.Parse(context.ContentType)];

            string text = GetDelimited(context.Object, delimiter);

            context.HttpContext.Response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
            context.HttpContext.Response.StatusCode = (int) HttpStatusCode.OK;
            await context.HttpContext.Response.WriteAsync(text);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        [Pure]
        [NotNull]
        private static string GetDelimited([CanBeNull] object value, char delimiter)
        {
            switch (value)
            {
                case null:
                {
                    return string.Empty;
                }
                case XDocument document:
                {
                    return document.ToDelimited(true, delimiter);
                }
                case XElement element:
                {
                    return new XElement[] { element }.ToDelimited(true, delimiter);
                }
                case IEnumerable<XElement> elements:
                {
                    return elements.ToDelimited(true, delimiter);
                }
                case IEnumerable<object> enumerable:
                {
                    return enumerable.ToDelimited(true, delimiter);
                }
                default:
                {
                    return new object[] { value }.ToDelimited(true, delimiter);
                }
            }
        }
    }
}