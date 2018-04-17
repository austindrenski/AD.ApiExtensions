﻿using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using AD.ApiExtensions.OutputFormatters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [PublicAPI]
    public class DelimitedOutputFormatterTests
    {
        [Theory]
        [InlineData("text/csv", ',')]
        [InlineData("text/psv", '|')]
        [InlineData("text/tab-separated-values", '\t')]
        public void Test0(string contentType, char delimiter)
        {
            HttpContext context =
                GetHttpContext(("Content-Type", contentType));

            OutputFormatterWriteContext outputContext =
                GetOutputFormatterWriteContext(
                    context,
                    new XElement[]
                    {
                        new XElement("record",
                            new XElement("a", 1),
                            new XElement("b", 2),
                            new XElement("c", 3))
                    });

            DelimitedOutputFormatter formatter = new DelimitedOutputFormatter();

            // Does the formatter accept the content type?
            Assert.True(formatter.CanWriteResult(outputContext));

            formatter.WriteAsync(outputContext).Wait();

            // Did the formatter set the response headers?
            Assert.Equal($"{contentType}; charset={Encoding.UTF8.WebName}", context.Response.ContentType);
            Assert.Equal(Encoding.UTF8.WebName, context.Response.Headers["charset"]);
            Assert.Equal("present", context.Response.Headers["header"]);
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);

            // Did the formatter write the response to the body?
            string expected =
                string.Join(Environment.NewLine,
                    $"a{delimiter}b{delimiter}c",
                    $"1{delimiter}2{delimiter}3");

            Assert.Equal(expected, GetBodyString(context.Response));
        }

        [Pure]
        [NotNull]
        public static HttpContext GetHttpContext(params (string Header, string Value)[] headers)
        {
            DefaultHttpContext context = new DefaultHttpContext();

            foreach ((string header, string value) in headers)
            {
                context.Request.Headers.Add(header, value);
            }

            context.Response.Body = new MemoryStream();

            return context;
        }

        [Pure]
        [NotNull]
        public static OutputFormatterWriteContext GetOutputFormatterWriteContext<T>([NotNull] HttpContext context, [NotNull] T value)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return
                new OutputFormatterWriteContext(context, (s, e) => new StreamWriter(s, e), typeof(T), value)
                {
                    ContentType = context.Request.ContentType
                };
        }

        /// <summary>
        /// Reads the response body with the encoding specified by the content type header.
        /// </summary>
        /// <param name="response">
        /// The response from which to read.
        /// </param>
        /// <returns>
        /// The string value of the response body.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [NotNull]
        public static string GetBodyString([NotNull] HttpResponse response)
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (response.Body.CanSeek)
            {
                response.Body.Seek(default, SeekOrigin.Begin);
            }

            Encoding encoding = MediaTypeHeaderValue.Parse(response.ContentType).Encoding;

            using (StreamReader reader = new StreamReader(response.Body, encoding))
            {
                return reader.ReadToEnd();
            }
        }
    }
}