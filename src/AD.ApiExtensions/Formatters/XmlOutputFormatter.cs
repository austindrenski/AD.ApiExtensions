using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AD.Xml;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AD.ApiExtensions.Formatters
{
    /// <inheritdoc cref="IOutputFormatter"/>
    /// <inheritdoc cref="IApiResponseTypeMetadataProvider"/>
    /// <summary>
    /// Writes an object in XML format to the output stream.
    /// </summary>
    [PublicAPI]
    public sealed class XmlOutputFormatter : IOutputFormatter, IApiResponseTypeMetadataProvider
    {
        /// <summary>
        /// The collection of supported media types.
        /// </summary>
        [NotNull]
        public IList<MediaType> SupportedMediaTypes { get; } = new List<MediaType>
        {
            new MediaType("application/xml")
        };

        /// <inheritdoc />
        [Pure]
        [NotNull]
        public IReadOnlyList<string> GetSupportedContentTypes([CanBeNull] string contentType, [CanBeNull] Type objectType)
        {
            MediaType mediaType = contentType != null ? new MediaType(contentType) : default;

            return
                SupportedMediaTypes.Where(x => CanWriteResult(x, mediaType))
                                   .Select(x => $"{x.Type}/{x.SubType}")
                                   .ToArray();
        }

        /// <inheritdoc />
        [Pure]
        public bool CanWriteResult([NotNull] OutputFormatterCanWriteContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            MediaType mediaType = new MediaType(context.ContentType);

            return SupportedMediaTypes.Any(x => CanWriteResult(x, mediaType));
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.HttpContext.Response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
            context.HttpContext.Response.StatusCode = (int) HttpStatusCode.OK;

            switch (context.Object)
            {
                case IEnumerable<XElement> elements:
                    await context.HttpContext.Response.WriteAsync(elements.ToXDocument().ToString());
                    return;

                case XDocument document:
                    await context.HttpContext.Response.WriteAsync(document.ToString());
                    return;

                case IEnumerable<object> collection:
                    await context.HttpContext.Response.WriteAsync(collection.ToXmlString());
                    return;

                default:
                    await context.HttpContext.Response.WriteAsync(new object[] { context.Object }.ToXmlString());
                    return;
            }
        }

        /// <summary>
        /// Determines whether this <see cref="IOutputFormatter" /> can produce the specified <paramref name="contentType"/>.
        /// </summary>
        /// <param name="supportedType">The content type that is supported.</param>
        /// <param name="contentType">The content type to check.</param>
        /// <returns>
        /// True if the formatter can write the response; otherwise, false.
        /// </returns>
        [Pure]
        static bool CanWriteResult(in MediaType supportedType, in MediaType contentType)
            => supportedType.HasWildcard && contentType.IsSubsetOf(supportedType) || supportedType.IsSubsetOf(contentType);
    }
}