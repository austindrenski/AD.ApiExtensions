using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Validates that a property contains three alphabetic characters.
    /// </summary>
    [PublicAPI]
    public class Alpha3Attribute : RegularExpressionAttribute
    {
        /// <summary>
        /// A pattern of 3 alphabetic characters.
        /// </summary>
        public const string StrictRegexPattern = "^[0-9]{3}$";

        /// <summary>
        /// A pattern of 3 alphanumeric characters.
        /// </summary>
        public const string RelaxedRegexPattern = "^[A-Z0-9]{3}$";

        /// <inheritdoc />
        /// <summary>
        /// Validates that a property contains three alphabetic characters.
        /// </summary>
        public Alpha3Attribute(bool strict = true) : base(strict ? StrictRegexPattern : RelaxedRegexPattern)
        {
        }
    }
}