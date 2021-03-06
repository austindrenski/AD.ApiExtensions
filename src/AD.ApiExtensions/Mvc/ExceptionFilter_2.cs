﻿using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace AD.ApiExtensions.Mvc
{
    /// <inheritdoc cref="ExceptionFilter{TException}"/>
    /// <summary>
    /// Handles a <typeparamref name="TException"/> by returning a <typeparamref name="TResult"/>.
    /// </summary>
    [PublicAPI]
    public class ExceptionFilter<TException, TResult> : ExceptionFilter<TException>
        where TException : Exception
        where TResult : StatusCodeResult, new()
    {
        /// <inheritdoc />
        [NotNull]
        public override Type Type => typeof(TResult);

        /// <inheritdoc />
        /// <summary>
        /// Constructs a <see cref="T:AD.ApiExtensions.Mvc.ExceptionFilter`2" /> with the specified order value.
        /// </summary>
        /// <param name="order">
        /// The order value for determining the order of execution of filters.
        /// </param>
        public ExceptionFilter(int order) : base(new TResult().StatusCode, order) {}
    }
}