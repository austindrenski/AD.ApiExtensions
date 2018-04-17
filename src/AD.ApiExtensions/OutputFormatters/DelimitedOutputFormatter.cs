using System;
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

namespace AD.ApiExtensions.OutputFormatters
{
    /// <inheritdoc cref="IOutputFormatter"/>
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
        public ISet<(MediaType MediaType, char Delimiter)> SupportedMediaTypes { get; }

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        public DelimitedOutputFormatter()
        {
            SupportedMediaTypes =
                new HashSet<(MediaType, char)>
                {
                    (new MediaType("text/csv"), ','),
                    (new MediaType("text/psv"), '|'),
                    (new MediaType("text/tab-separated-values"), '\t')
                };
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetSupportedContentTypes([NotNull] string contentType, [NotNull] Type objectType)
        {
            if (contentType is null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (objectType is null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            MediaType mediaType = new MediaType(contentType);

            return SupportedMediaTypes.Where(x => CanWriteResult(x.MediaType, mediaType)).Select(x => $"{x.MediaType.Type}/{x.MediaType.SubType}").ToArray();
        }

        /// <inheritdoc />
        [Pure]
        public bool CanWriteResult([NotNull] OutputFormatterCanWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return CanWriteResult(new MediaType(context.ContentType));
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string text = GetDelimited(context.Object, GetDelimiter(context.ContentType));

            context.HttpContext.Response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
            context.HttpContext.Response.Headers.Add("header", "present");
            context.HttpContext.Response.Headers.Add("charset", Encoding.UTF8.WebName);
            await context.HttpContext.Response.WriteAsync(text, Encoding.UTF8, context.HttpContext.RequestAborted);
        }

        [Pure]
        private char GetDelimiter(StringSegment contentType)
        {
            MediaType mediaType = new MediaType(contentType);
            return SupportedMediaTypes.First(x => x.MediaType.IsSubsetOf(mediaType) || mediaType.IsSubsetOf(x.MediaType)).Delimiter;
        }

        /// <summary>
        /// Gets the delimited string representation of the value.
        /// </summary>
        /// <param name="value">
        /// The value to serialize as a delimited string.
        /// </param>
        /// <param name="delimiter">
        /// The delimiter character.
        /// </param>
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

        /// <summary>
        /// Determines whether this <see cref="IOutputFormatter" /> can produce the specified <paramref name="contentType"/>.
        /// </summary>
        /// <param name="supportedType">
        /// The content type that is supported.
        /// </param>
        /// <param name="contentType">
        /// The content type to check.
        /// </param>
        /// <returns>
        /// True if the formatter can write the response; otherwise, false.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Pure]
        private static bool CanWriteResult(MediaType supportedType, MediaType contentType)
        {
            if (supportedType.HasWildcard && contentType.IsSubsetOf(supportedType))
            {
                return true;
            }

            return supportedType.IsSubsetOf(contentType);
        }

        /// <summary>
        /// Determines whether this <see cref="IOutputFormatter" /> can produce the specified <paramref name="contentType"/>.
        /// </summary>
        /// <param name="contentType">
        /// The content type to check.
        /// </param>
        /// <returns>
        /// True if the formatter can write the response; otherwise, false.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Pure]
        private bool CanWriteResult(MediaType contentType)
        {
            foreach ((MediaType supportedType, char _) in SupportedMediaTypes)
            {
                if (CanWriteResult(supportedType, contentType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}