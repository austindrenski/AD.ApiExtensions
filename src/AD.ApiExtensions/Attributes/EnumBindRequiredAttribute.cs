using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Attributes
{
    /// <inheritdoc cref="ValidationAttribute" />
    /// <summary>
    /// Requires that an enum parameter be bound against a non-default value.
    /// </summary>
    [PublicAPI]
    public class EnumBindRequiredAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult IsValid([NotNull] object value, [NotNull] ValidationContext validationContext)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (validationContext is null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            if (!value.GetType().IsEnum)
            {
                throw new ArgumentException("Enumeration type required.");
            }

            return
                (long) value is default
                    ? new ValidationResult($"Expected non-default enumeration, but received: {value}")
                    : ValidationResult.Success;
        }
    }
}