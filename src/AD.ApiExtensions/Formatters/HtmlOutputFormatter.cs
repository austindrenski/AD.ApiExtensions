using System;
using System.IO;
using System.Linq;
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

namespace AD.ApiExtensions.Formatters
{
    /// <inheritdoc/>
    /// <summary>
    /// Writes an object in HTML format to the output stream.
    /// </summary>
    [PublicAPI]
    public class HtmlOutputFormatter<T> : OutputFormatter
    {
        /// <summary>
        /// The view path.
        /// </summary>
        [NotNull] readonly string _view;

        /// <summary>
        /// The delegate returning a model.
        /// </summary>
        [NotNull] readonly Func<HttpContext, object, T> _modelFactory;

        /// <summary>
        /// Constructs a new <see cref="HtmlOutputFormatter{T}"/>.
        /// </summary>
        /// <param name="view">The view path.</param>
        /// <param name="modelFactory">The delegate returning a model.</param>
        /// <exception cref="ArgumentNullException"><paramref name="view"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="modelFactory"/></exception>
        public HtmlOutputFormatter([NotNull] [PathReference] string view, [NotNull] Func<HttpContext, object, T> modelFactory)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (modelFactory == null)
                throw new ArgumentNullException(nameof(modelFactory));

            _view = view;
            _modelFactory = modelFactory;
        }

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

            ActionContext actionContext =
                new ActionContext(
                    httpContext,
                    httpContext.GetRouteData(),
                    new ActionDescriptor());

            IServiceProvider services = httpContext.RequestServices;

            ViewEngineResult viewEngineResult =
                services.GetRequiredService<IRazorViewEngine>()
                        .FindView(actionContext, _view, false);

            if (!viewEngineResult.Success)
                throw new FileNotFoundException(_view);

            IView view = viewEngineResult.View;

            ViewDataDictionary viewDataDictionary =
                new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = _modelFactory(httpContext, context.Object)
                };

            TempDataDictionary tempDataDictionary =
                new TempDataDictionary(httpContext, services.GetRequiredService<ITempDataProvider>());

            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext =
                    new ViewContext(
                        actionContext,
                        view,
                        viewDataDictionary,
                        tempDataDictionary,
                        writer,
                        new HtmlHelperOptions());

                await view.RenderAsync(viewContext);
                await httpContext.Response.WriteAsync(writer.ToString(), Encoding.UTF8, httpContext.RequestAborted);
            }
        }
    }
}