using System;
using System.Threading.Tasks;
using AD.ApiExtensions.Primitives;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.ApiExtensions.ModelBinders
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a model binder that creates <see cref="GroupingValues{TKey, TValue}" /> instances from valid objects.
    /// </summary>
    [PublicAPI]
    public sealed class GroupingValuesModelBinder : IModelBinder
    {
        /// <inheritdoc />
        [NotNull]
        public Task BindModelAsync([NotNull] ModelBindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            ValueProviderResult valueProviderResult =
                context.ValueProvider.GetValue(context.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            context.ModelState.SetModelValue(context.ModelName, valueProviderResult);

            GroupingValues<string, string> model =
                GroupingValues<string, string>.Parse(valueProviderResult.Values);

            context.Result =
                ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }
    }
}