using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
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
        [NotNull] private static readonly IReadOnlyList<MediaTypeHeaderValue> SupportedMediaTypes;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static DelimitedOutputFormatter()
        {
            SupportedMediaTypes =
                new MediaTypeHeaderValue[]
                {
                    MediaTypeHeaderValue.Parse("text/csv"),
                    MediaTypeHeaderValue.Parse("text/psv"),
                    MediaTypeHeaderValue.Parse("text/tsv")
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
                SupportedMediaTypes.Contains(MediaTypeHeaderValue.Parse(context.ContentType)) ||
                context.HttpContext.Request.Headers["user-agent"].Any(x => x?.StartsWith("Stata") ?? false);
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            char delimiter =
                GetDelimiter(context.ContentType);

            IReadOnlyList<object> results =
                context.Object as IReadOnlyList<object> ??
                (context.Object as IEnumerable<object>)?.ToArray() ??
                new object[] { context.Object };

            string headers =
                results.DefaultIfEmpty(new object())
                       .First()
                       .GetType()
                       .GetProperties()
                       .Select(x => x.Name)
                       .ToDelimited(delimiter);

            string delimited = results.ToDelimited(delimiter);

            using (StringWriter writer = new StringWriter())
            {
                await writer.WriteLineAsync(headers);
                await writer.WriteLineAsync(delimited);

                context.HttpContext.Response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.HttpContext.Response.WriteAsync(writer.ToString());
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        [Pure]
        private static char GetDelimiter(StringSegment value)
        {
            MediaType mediaType = new MediaType(value);
            switch (mediaType.SubType.Value)
            {
                case "psv":
                {
                    return '|';
                }
                case "tsv":
                {
                    return '\t';
                }
                case "csv":
                default:
                {
                    return ',';
                }
            }
        }
    }
}