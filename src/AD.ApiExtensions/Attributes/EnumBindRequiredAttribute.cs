using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Attributes
{
    /// <inheritdoc cref="ValidationAttribute" />
    /// <summary>
    /// Requires that an int-based enum parameter be bound against a non-default value.
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
                (int) value is 0
                    ? new ValidationResult($"Expected non-default enumeration, but received: {value}")
                    : ValidationResult.Success;
        }
    }
}