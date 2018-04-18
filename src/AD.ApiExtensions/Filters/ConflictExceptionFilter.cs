using System;
using AD.ApiExtensions.Http;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Filters
{
    /// <inheritdoc cref="MapExceptionFilter{TException, TResult}" />
    /// <summary>
    /// Handles an exception by returning a <see cref="ConflictResult"/>.
    /// </summary>
    [PublicAPI]
    public class ConflictExceptionFilter<T> : MapExceptionFilter<T, ConflictResult> where T : Exception
    {
    }
}