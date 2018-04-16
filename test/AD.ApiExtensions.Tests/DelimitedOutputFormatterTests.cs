using System;
using System.IO;
using System.Xml.Linq;
using AD.ApiExtensions.OutputFormatters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
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
            DelimitedOutputFormatter formatter = new DelimitedOutputFormatter();

            DefaultHttpContext context = new DefaultHttpContext();

            context.Initialize(context.Features);

            context.Request.Headers["Content-Type"] = contentType;

            OutputFormatterWriteContext outputContext =
                new OutputFormatterWriteContext(
                    context,
                    (s, e) => new StreamWriter(s, e),
                    typeof(XElement[]),
                    new XElement[]
                    {
                        new XElement("a", 1),
                        new XElement("b", 2),
                        new XElement("c", 3),
                    })
                {
                    ContentType = context.Request.ContentType
                };

            Assert.True(formatter.CanWriteResult(outputContext));

            formatter.WriteAsync(outputContext).Wait();

            Assert.Equal($"{contentType}; charset=utf-8", context.Response.Headers["Content-Type"]);
            Assert.Equal($"{contentType}; charset=utf-8", context.Response.ContentType);
            Assert.Equal("utf-8", context.Response.Headers["charset"]);
            Assert.Equal("present", context.Response.Headers["header"]);
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);

            using (StreamReader reader = new StreamReader(context.Response.Body))
            {
                string expected =
                    string.Join(
                        Environment.NewLine,
                        $"a{delimiter}b{delimiter}c",
                        $"1{delimiter}2{delimiter}3");

                Assert.Equal(expected, reader.ReadToEnd());
            }
        }
    }
}