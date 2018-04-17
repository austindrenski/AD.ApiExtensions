using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

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
        public IDictionary<string, char> SupportedMediaTypes { get; }

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        public DelimitedOutputFormatter()
        {
            SupportedMediaTypes =
                new Dictionary<string, char>
                {
                    ["text/csv; charset=utf-8"] = ',',
                    ["text/psv; charset=utf-8"] = '|',
                    ["text/tab-separated-values; charset=utf-8"] = '\t'
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

            if (!SupportedMediaTypes.Any())
            {
                throw new InvalidOperationException($"{nameof(DelimitedOutputFormatter)} does not support any media types.");
            }

            List<MediaType> results = new List<MediaType>();

            MediaType contentMediaType = new MediaType(contentType);

            foreach ((string mediaType, char _) in SupportedMediaTypes)
            {
                MediaType supportedMediaType = new MediaType(mediaType);

                if (supportedMediaType.HasWildcard && contentMediaType.IsSubsetOf(supportedMediaType))
                {
                    results.Add(contentMediaType);
                }
                else if (supportedMediaType.IsSubsetOf(contentMediaType))
                {
                    results.Add(supportedMediaType);
                }
            }

            return results.Select(x => $"{x.Type}/{x.SubType}").ToArray();
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
        public bool CanWriteResult(StringSegment contentType)
        {
            if (!SupportedMediaTypes.Any())
            {
                throw new InvalidOperationException($"{nameof(DelimitedOutputFormatter)} does not support any media types.");
            }

            MediaType contentMediaType = new MediaType(contentType);

            foreach ((string mediaType, char _) in SupportedMediaTypes)
            {
                MediaType supportedMediaType = new MediaType(mediaType);

                if (supportedMediaType.HasWildcard && contentMediaType.IsSubsetOf(supportedMediaType))
                {
                    return true;
                }

                if (supportedMediaType.IsSubsetOf(contentMediaType))
                {
                    return true;
                }
            }

            return false;
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
                CanWriteResult(context.ContentType) ||
                context.HttpContext.Request.Headers["user-agent"].Any(x => x?.StartsWith("Stata") ?? false);
        }

        /// <inheritdoc />
        public async Task WriteAsync([NotNull] OutputFormatterWriteContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            MediaType headerValue = new MediaType(context.ContentType.Value);
            string text = GetDelimited(context.Object, SupportedMediaTypes[context.ContentType.Value]);

            context.HttpContext.Response.ContentType = context.ContentType.Value;
            context.HttpContext.Response.Headers.Add("header", "present");
            context.HttpContext.Response.Headers.Add("charset", headerValue.Encoding.WebName);
            await context.HttpContext.Response.WriteAsync(text, headerValue.Encoding, context.HttpContext.RequestAborted);
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
    }
}