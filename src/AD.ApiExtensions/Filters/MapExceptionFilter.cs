using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace AD.ApiExtensions.Filters
{
    /// <inheritdoc cref="IAsyncActionFilter"/>
    /// <inheritdoc cref="IExceptionFilter"/>
    /// <inheritdoc cref="IOrderedFilter"/>
    /// <summary>
    /// Handles a <typeparamref name="TException"/> by returning a <see cref="TResult"/>.
    /// </summary>
    [PublicAPI]
    public class MapExceptionFilter<TException, TResult>
        : IAsyncExceptionFilter,
          IExceptionFilter,
          IOrderedFilter
        where TException : Exception
        where TResult : StatusCodeResult, new()
    {
        /// <inheritdoc />
        public int Order { get; }

        /// <summary>
        /// Constructs a <see cref="MapExceptionFilter{TException,TResult}"/>.
        /// </summary>
        public MapExceptionFilter()
        {
        }

        /// <summary>
        /// Constructs a <see cref="MapExceptionFilter{TException,TResult}"/> with the specified order value.
        /// </summary>
        /// <param name="order">
        /// The order value for determining the order of execution of filters.
        /// </param>
        public MapExceptionFilter(int order)
        {
            Order = order;
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