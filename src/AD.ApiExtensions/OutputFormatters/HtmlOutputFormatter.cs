using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace AD.ApiExtensions.OutputFormatters
{
    /// <inheritdoc />
    /// <summary>
    /// Writes an object in HTML format to the output stream.
    /// </summary>
    [PublicAPI]
    public sealed class HtmlOutputFormatter<T> : IOutputFormatter
    {
        /// <summary>
        /// The collection of supported media types.
        /// </summary>
        [NotNull] private static readonly IReadOnlyList<MediaTypeHeaderValue> SupportedMediaTypes;

        /// <summary>
        /// The view path.
        /// </summary>
        [NotNull] private readonly string _view;

        /// <summary>
        /// The delegate returning a model.
        /// </summary>
        [NotNull] private readonly Func<HttpContext, object, T> _modelFactory;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static HtmlOutputFormatter()
        {
            SupportedMediaTypes =
                new MediaTypeHeaderValue[]
                {
                    MediaTypeHeaderValue.Parse("text/html"),
                    MediaTypeHeaderValue.Parse("text/xhtml")
                };
        }

        /// <inheritdoc />
        public HtmlOutputFormatter([NotNull] [PathReference] string view, [NotNull] Func<HttpContext, object, T> modelFactory)
        {
            if (view is null)
            {
                throw new ArgumentNullException(nameof(view));
            }
            if (modelFactory is null)
            {
                throw new ArgumentNullException(nameof(modelFactory));
            }

            _view = view;
            _modelFactory = modelFactory;
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

            ActionContext actionContext =
                new ActionContext(
                    context.HttpContext,
                    context.HttpContext.GetRouteData(),
                    new ActionDescriptor());

            IRazorViewEngine razorViewEngine = context.HttpContext.RequestServices.GetService<IRazorViewEngine>();
            ITempDataProvider tempDataProvider = context.HttpContext.RequestServices.GetService<ITempDataProvider>();

            ViewEngineResult viewEngineResult = razorViewEngine.FindView(actionContext, _view, false);

            if (!viewEngineResult.Success)
            {
                throw new FileNotFoundException(_view);
            }

            ViewDataDictionary viewDataDictionary =
                new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = _modelFactory(context.HttpContext, context.Object)
                };

            TempDataDictionary tempDataDictionary =
                new TempDataDictionary(context.HttpContext, tempDataProvider);

            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext =
                    new ViewContext(
                        actionContext,
                        viewEngineResult.View,
                        viewDataDictionary,
                        tempDataDictionary,
                        writer,
                        new HtmlHelperOptions());

                await viewEngineResult.View.RenderAsync(viewContext);

                context.HttpContext.Response.ContentType = MediaType.ReplaceEncoding(context.ContentType, Encoding.UTF8);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.HttpContext.Response.WriteAsync(writer.ToString());
            }
        }
    }
}