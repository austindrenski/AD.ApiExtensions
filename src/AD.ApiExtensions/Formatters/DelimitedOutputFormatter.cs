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
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.Formatters
{
    /// <inheritdoc cref="IOutputFormatter"/>
    /// <inheritdoc cref="IApiResponseTypeMetadataProvider"/>
    /// <summary>
    /// Writes an object in delimited format to the output stream.
    /// </summary>
    [PublicAPI]
    public class DelimitedOutputFormatter : IOutputFormatter, IApiResponseTypeMetadataProvider
    {
        /// <summary>
        /// The collection of supported media types.
        /// </summary>
        [NotNull]
        public IList<(MediaType MediaType, char Delimiter)> SupportedMediaTypes { get; } = new List<(MediaType, char)>
        {
            (new MediaType("text/csv"), ','),
            (new MediaType("text/psv"), '|'),
            (new MediaType("text/tab-separated-values"), '\t')
        };

        /// <inheritdoc />
        [Pure]
        [NotNull]
        public IReadOnlyList<string> GetSupportedContentTypes([CanBeNull] string contentType, [CanBeNull] Type objectType)
        {
            MediaType mediaType = contentType != null ? new MediaType(contentType) : default;

            return
                SupportedMediaTypes.Where(x => CanWriteResult(x.MediaType, mediaType))
                                   .Select(x => $"{x.MediaType.Type}/{x.MediaType.SubType}")
                                   .ToArray();
        }

        /// <inheritdoc />
        [Pure]
        public bool CanWriteResult([NotNull] OutputFormatterCanWriteContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            MediaType mediaType = new MediaType(context.ContentType);

            return SupportedMediaTypes.Any(x => CanWriteResult(x.MediaType, mediaType));
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            string text = GetDelimited(context.Object, GetDelimiter(context.ContentType));

            context.HttpContext.Response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
            context.HttpContext.Response.Headers.Add("header", "present");
            context.HttpContext.Response.Headers.Add("charset", Encoding.UTF8.WebName);
            await context.HttpContext.Response.WriteAsync(text, Encoding.UTF8, context.HttpContext.RequestAborted);
        }

        [Pure]
        char GetDelimiter(in StringSegment contentType)
        {
            MediaType mediaType = new MediaType(contentType);
            return SupportedMediaTypes.First(x => x.MediaType.IsSubsetOf(mediaType) || mediaType.IsSubsetOf(x.MediaType)).Delimiter;
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