using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

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
        /// Initializes the <see cref="KebabBindingMetadataProvider"/>
        /// </summary>
        /// <param name="home">
        /// The string for the controller referencing the server itself.
        /// </param>
        /// <param name="index">
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public KebabControllerModelConvention([NotNull] string home = "Home", [NotNull] string index = "Index")
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

// TODO: is there any good case for this?
//                foreach (ParameterModel parameter in action.Parameters)
//                {
//                    if (parameter.BindingInfo is null)
//                    {
//                        parameter.BindingInfo = new BindingInfo();
//                    }
//
//                    parameter.BindingInfo.BinderModelName = parameter.ParameterName.CamelCaseToKebabCase();
//                }
            }
        }
    }
}