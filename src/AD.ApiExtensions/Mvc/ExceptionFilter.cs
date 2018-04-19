using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace AD.ApiExtensions.Mvc
{
    /// <inheritdoc cref="IAsyncActionFilter"/>
    /// <inheritdoc cref="IExceptionFilter"/>
    /// <inheritdoc cref="IOrderedFilter"/>
    /// <summary>
    /// Handles a <typeparamref name="TException"/> by returning a <see cref="TResult"/>.
    /// </summary>
    [PublicAPI]
    public class ExceptionFilter<TException, TResult>
        : IAsyncExceptionFilter,
          IExceptionFilter,
          IOrderedFilter,
          IApiResponseMetadataProvider
        where TException : Exception
        where TResult : StatusCodeResult, new()
    {
        /// <inheritdoc />
        public int Order { get; }

        /// <inheritdoc />
        public Type Type => typeof(TResult);

        /// <inheritdoc />
        public int StatusCode => new TResult().StatusCode;

        /// <summary>
        /// Constructs a <see cref="ExceptionFilter{TException,TResult}"/>.
        /// </summary>
        public ExceptionFilter()
        {
        }

        /// <summary>
        /// Constructs a <see cref="ExceptionFilter{TException,TResult}"/> with the specified order value.
        /// </summary>
        /// <param name="order">
        /// The order value for determining the order of execution of filters.
        /// </param>
        public ExceptionFilter(int order)
        {
            Order = order;
        }

        /// <inheritdoc />
        public virtual void SetContentTypes([NotNull] MediaTypeCollection contentTypes)
        {
            if (contentTypes is null)
            {
                throw new ArgumentNullException(nameof(contentTypes));
            }
        }

        /// <inheritdoc />
        public virtual void OnException([NotNull] ExceptionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!(context.Exception is TException))
            {
                return;
            }

            context.ExceptionHandled = true;
            context.Result = new TResult();
        }

        /// <inheritdoc />
        public virtual Task OnExceptionAsync([NotNull] ExceptionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            OnException(context);

            return Task.CompletedTask;
        }
    }
}