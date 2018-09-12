using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AD.Xml;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace AD.ApiExtensions.Formatters
{
    /// <inheritdoc/>
    /// <summary>
    /// Writes an object in XML format to the output stream.
    /// </summary>
    [PublicAPI]
    public class XmlOutputFormatter : OutputFormatter
    {
        /// <summary>
        /// Add a content type to the collection of supported media types.
        /// </summary>
        /// <param name="contentType">The content type to register.</param>
        /// <exception cref="ArgumentNullException"><paramref name="contentType"/></exception>
        public void Add([NotNull] string contentType)
        {
            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(contentType));
        }

        /// <inheritdoc />
        public override void WriteResponseHeaders([NotNull] OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            HttpResponse response = context.HttpContext.Response;
            response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
        }

        /// <inheritdoc />
        [NotNull]
        public override async Task WriteResponseBodyAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            HttpContext httpContext = context.HttpContext;
            HttpResponse response = httpContext.Response;

            switch (context.Object)
            {
                case IEnumerable<XElement> e:
                    await response.WriteAsync(e.ToXDocument().ToString(), Encoding.UTF8, httpContext.RequestAborted);
                    return;

                case XDocument d:
                    await response.WriteAsync(d.ToString(), Encoding.UTF8, httpContext.RequestAborted);
                    return;

                case IEnumerable<object> c:
                    await response.WriteAsync(c.ToXmlString(), Encoding.UTF8, httpContext.RequestAborted);
                    return;

                default:
                    await response.WriteAsync(new object[] { context.Object }.ToXmlString(), Encoding.UTF8, httpContext.RequestAborted);
                    return;
            }
        }
    }
}