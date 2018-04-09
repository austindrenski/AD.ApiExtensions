using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.ApiExtensions.Conventions
{
    /// <inheritdoc />
    /// <summary>
    /// Provides conventional mapping from camel case to kebab case for controller and action routing.
    /// </summary>
    [PublicAPI]
    public sealed class KebabControllerModelConvention : IControllerModelConvention
    {
        /// <summary>
        /// The string for the controller referencing the server itself.
        /// </summary>
        [NotNull] private readonly string _home;

        /// <summary>
        /// The string for the controller action referencing the controller itself.
        /// </summary>
        [NotNull] private readonly string _index;

        /// <summary>
        /// True if actions that have both an <see cref="HttpGetAttribute"/> and an <see cref="HttpPostAttribute"/>
        /// should be configure to use <see cref="BindingSource.Query"/> and <see cref="BindingSource.Form"/>, respectively.
        /// </summary>
        private readonly bool _bindQueryOrForm;

        /// <summary>
        /// Initializes the <see cref="KebabBindingMetadataProvider"/>
        /// </summary>
        /// <param name="home">
        /// The string for the controller referencing the server itself.
        /// </param>
        /// <param name="index">
        /// </param>
        /// <param name="bindQueryOrForm">
        /// True if actions that have both an <see cref="HttpGetAttribute"/> and an <see cref="HttpPostAttribute"/>
        /// should be configure to use <see cref="BindingSource.Query"/> and <see cref="BindingSource.Form"/>, respectively.
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public KebabControllerModelConvention([NotNull] string home = "Home", [NotNull] string index = "Index", bool bindQueryOrForm = true)
        {
            if (home is null)
            {
                throw new ArgumentNullException(nameof(home));
            }

            if (index is null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            _home = home;
            _index = index;
            _bindQueryOrForm = bindQueryOrForm;
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
                if (selector.AttributeRouteModel is null)
                {
                    selector.AttributeRouteModel = new AttributeRouteModel();
                }

                if (selector.AttributeRouteModel.Template != null)
                {
                    continue;
                }

                selector.AttributeRouteModel.Template =
                    controller.ControllerName.Equals(_home, StringComparison.Ordinal)
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
                        action.ActionName.Equals(_index, StringComparison.Ordinal)
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

                    if (_bindQueryOrForm)
                    {
                        parameter.BindingInfo.BindingSource =
                            action.Attributes.OfType<HttpGetAttribute>().Any() &&
                            action.Attributes.OfType<HttpPostAttribute>().Any()
                                ? BindingSource.Form
                                : BindingSource.Query;
                    }
                }
            }
        }
    }
}