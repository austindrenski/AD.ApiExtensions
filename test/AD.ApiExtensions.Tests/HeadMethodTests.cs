using System.IO;
using System.Text;
using System.Threading.Tasks;
using AD.ApiExtensions.Http;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace AD.ApiExtensions.Tests
{
    [PublicAPI]
    public class HeadMethodTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("Success")]
        public async Task SendResponseBody(string value)
        {
            HttpContext context = GetHttpContext(HttpMethods.Head);

            HeadMethod head = new HeadMethod();

            await
                head.InvokeAsync(
                    context,
                    async ctx =>
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(value);
                        await ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    });

            Assert.Equal(value.Length, context.Response.Body.Length);
            Assert.Equal(-1, context.Response.Body.ReadByte());
        }

        [Theory]
        [InlineData("")]
        [InlineData("Success")]
        public async Task DontSendResponseBody(string value)
        {
            HttpContext context = GetHttpContext(HttpMethods.Head);

            HeadMethod head = new HeadMethod(false);

            await
                head.InvokeAsync(
                    context,
                    async ctx =>
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(value);
                        await ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    });

            Assert.Equal(0, context.Response.Body.Length);
            Assert.Equal(-1, context.Response.Body.ReadByte());
        }

        [Pure]
        [NotNull]
        public static HttpContext GetHttpContext(string httpMethod, [NotNull] params (string Header, string Value)[] headers)
        {
            DefaultHttpContext context = new DefaultHttpContext();

            context.Request.Method = httpMethod;

            foreach ((string header, string value) in headers)
            {
                context.Request.Headers.Add(header, value);
            }

            context.Response.Body = new MemoryStream();

            return context;
        }
    }
}