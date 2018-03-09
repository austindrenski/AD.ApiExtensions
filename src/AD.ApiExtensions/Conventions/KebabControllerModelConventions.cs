using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.ApiExtensions.Conventions
{
    // TODO: document KebabControllerModelConventions.
    /// <inheritdoc />
    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public sealed class KebabControllerModelConvention : IControllerModelConvention
    {
        /// <summary>
        ///
        /// </summary>
        [NotNull] private static readonly string Index = "Index";

        /// <summary>
        ///
        /// </summary>
        [NotNull] private static readonly Regex HomeRegex = new Regex("Api\\b");

        /// <summary>
        ///
        /// </summary>
        [NotNull] private readonly string _home;

        /// <summary>
        ///
        /// </summary>
        private readonly bool _respectAttributeId;

        ///  <summary>
        ///
        ///  </summary>
        ///  <param name="home">
        ///
        ///  </param>
        /// <param name="respectAttributeId">
        ///
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public KebabControllerModelConvention([NotNull] string home, bool respectAttributeId = false)
        {
            if (home is null)
            {
                throw new ArgumentNullException(nameof(home));
            }

            _home = HomeRegex.Replace(home, string.Empty);
            _respectAttributeId = respectAttributeId;
        }

        /// <inheritdoc />
        public void Apply([NotNull] ControllerModel controller)
        {
            if (controller is null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            foreach (SelectorModel selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel is default)
                {
                    selector.AttributeRouteModel = new AttributeRouteModel();
                }

                if (selector.AttributeRouteModel.Template != null)
                {
                    continue;
                }

                selector.AttributeRouteModel.Template =
                    controller.ControllerName.Equals(_home, StringComparison.OrdinalIgnoreCase)
                        ? string.Empty
                        : controller.ControllerName.CamelCaseToKebabCase();
            }

            foreach (ActionModel action in controller.Actions)
            {
                foreach (SelectorModel selector in action.Selectors)
                {
                    if (selector.AttributeRouteModel is null)
                    {
                        selector.AttributeRouteModel = new AttributeRouteModel();
                    }

                    if (selector.AttributeRouteModel.Template != null)
                    {
                        continue;
                    }

                    selector.AttributeRouteModel.Template =
                        action.ActionName.Equals(Index, StringComparison.OrdinalIgnoreCase)
                            ? string.Empty
                            : action.ActionName.CamelCaseToPathCase();
                }

                foreach (ParameterModel parameter in action.Parameters)
                {
                    if (!parameter.ParameterInfo.ParameterType.IsPrimitive && parameter.ParameterInfo.ParameterType != typeof(string))
                    {
                        continue;
                    }

                    if (parameter.BindingInfo is null)
                    {
                        parameter.BindingInfo = new BindingInfo();
                    }

                    parameter.BindingInfo.BinderModelName = parameter.ParameterName.CamelCaseToKebabCase();

                    parameter.BindingInfo.BindingSource =
                        parameter.Action.Attributes.OfType<HttpPostAttribute>().Any()
                            ? BindingSource.Form
                            : BindingSource.Query;
                }
            }
        }
    }
}