using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace AD.ApiExtensions.Formatters
{
    /// <inheritdoc/>
    /// <summary>
    /// Writes an object in delimited format to the output stream.
    /// </summary>
    [PublicAPI]
    public class DelimitedOutputFormatter : OutputFormatter
    {
        /// <summary>
        /// The mapping of content types to delimiters.
        /// </summary>
        [NotNull] readonly Dictionary<StringSegment, char> _delimiters = new Dictionary<StringSegment, char>();

        /// <summary>
        /// Add a content type and delimiter to the collection of supported media types.
        /// </summary>
        /// <param name="contentType">The content type to register.</param>
        /// <param name="delimiter">The delimiter to register.</param>
        /// <exception cref="ArgumentNullException"><paramref name="contentType"/></exception>
        public void Add([NotNull] string contentType, char delimiter)
        {
            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(contentType));
            _delimiters[contentType] = delimiter;
        }

        /// <inheritdoc />
        public override void WriteResponseHeaders([NotNull] OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            HttpResponse response = context.HttpContext.Response;
            response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
            response.Headers.Add("header", "present");
            response.Headers.Add("charset", Encoding.UTF8.WebName);
        }

        /// <inheritdoc />
        [NotNull]
        public override async Task WriteResponseBodyAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            HttpContext httpContext = context.HttpContext;
            HttpResponse response = httpContext.Response;

            string text = GetDelimited(context.Object, _delimiters[context.ContentType]);
            await response.WriteAsync(text, Encoding.UTF8, httpContext.RequestAborted);
        }

        /// <summary>
        /// Gets the delimited string representation of the value.
        /// </summary>
        /// <param name="value">The value to serialize as a delimited string.</param>
        /// <param name="delimiter">The delimiter character.</param>
        [Pure]
        [NotNull]
        static string GetDelimited([CanBeNull] object value, char delimiter)
        {
            switch (value)
            {
                case null:
                    return string.Empty;

                case XDocument document:
                    return document.ToDelimited(delimiter);

                case XElement element:
                    return new XElement[] { element }.ToDelimited(true, delimiter);

                case IEnumerable<XElement> elements:
                    return elements.ToDelimited(true, delimiter);

                case IEnumerable enumerable:
                    return enumerable.Cast<object>().ToDelimited(true, delimiter);

                default:
                    return new object[] { value }.ToDelimited(true, delimiter);
            }
        }
    }
}