using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Attributes
{
    /// <inheritdoc cref="ValidationAttribute" />
    /// <summary>
    /// Requires that the <see cref="object.ToString()"/> representation to match a specified regular expression.
    /// </summary>
    [PublicAPI]
    public class PatternValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// The pattern for validation.
        /// </summary>
        [NotNull] private readonly Regex _regex;

        /// <summary>
        /// Constructs a <see cref="PatternValidationAttribute"/>.
        /// </summary>
        /// <param name="pattern">
        /// The pattern for validation.
        /// </param>
        /// <param name="options">
        /// The options applied to the regular expression.
        /// </param>
        public PatternValidationAttribute([NotNull] [RegexPattern] string pattern, RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            _regex = new Regex(pattern, options);
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid([NotNull] object value, [NotNull] ValidationContext context)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return
                _regex.IsMatch(value.ToString())
                    ? ValidationResult.Success
                    : new ValidationResult($"Expected {context.DisplayName} to match a pattern like '{_regex}', but received: '{value}'");
        }
    }
}