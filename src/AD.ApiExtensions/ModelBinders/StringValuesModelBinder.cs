using System;
using System.Linq;
using System.Threading.Tasks;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.ModelBinders
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a model binder that creates <see cref="StringValues" /> instances from valid objects.
    /// </summary>
    [PublicAPI]
    public sealed class StringValuesModelBinder : IModelBinder
    {
        /// <inheritdoc />
        [NotNull]
        public Task BindModelAsync([NotNull] ModelBindingContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            ValueProviderResult valueProviderResult =
                context.ValueProvider.GetValue(context.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            ValueProviderResult result =
                new ValueProviderResult(
                    Delimiter.Comma
                             .Split(valueProviderResult.Values)
                             .Select(x => x.Value)
                             .ToArray());

            context.ModelState.SetModelValue(context.ModelName, result);

            context.Result = ModelBindingResult.Success(result.Values);

            return Task.CompletedTask;
        }
    }
}