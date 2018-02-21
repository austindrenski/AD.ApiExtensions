using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AD.Xml;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace AD.ApiExtensions.OutputFormatters
{
    /// <inheritdoc />
    /// <summary>
    /// Writes an object in XML format to the output stream.
    /// </summary>
    [PublicAPI]
    public sealed class XmlOutputFormatter : IOutputFormatter
    {
        /// <summary>
        /// The collection of supported media types.
        /// </summary>
        [NotNull] private static readonly IReadOnlyList<MediaTypeHeaderValue> SupportedMediaTypes;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static XmlOutputFormatter()
        {
            SupportedMediaTypes =
                new MediaTypeHeaderValue[]
                {
                    MediaTypeHeaderValue.Parse("application/xml"),
                    MediaTypeHeaderValue.Parse("text/xml")
                };
        }

        /// <inheritdoc />
        public bool CanWriteResult([NotNull] OutputFormatterCanWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return SupportedMediaTypes.Contains(MediaTypeHeaderValue.Parse(context.ContentType));
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using (StringWriter writer = new StringWriter())
            {
                if (context.Object is IEnumerable<object> collection)
                {
                    await writer.WriteAsync(collection.ToXmlString());
                }
                else
                {
                    await writer.WriteAsync(new object[] { context.Object }.ToXmlString());
                }

                context.HttpContext.Response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.HttpContext.Response.WriteAsync(writer.ToString());
            }
        }
    }
}