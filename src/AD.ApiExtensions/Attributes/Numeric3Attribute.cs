using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Validates that a property contains three numeric characters.
    /// </summary>
    [PublicAPI]
    public class Numeric3Attribute : RegularExpressionAttribute
    {
        /// <summary>
        /// A pattern of 3 numeric characters.
        /// </summary>
        public const string StrictRegexPattern = "^[0-9]{3}$";

        /// <summary>
        /// A pattern of 3 alphanumeric characters.
        /// </summary>
        public const string RelaxedRegexPattern = "^[A-Z0-9]{3}$";

        /// <inheritdoc />
        /// <summary>
        /// Validates that a property contains three numeric characters.
        /// </summary>
        /// <param name="strict">
        /// True for <see cref="StrictRegexPattern"/>; otherwise <see cref="RelaxedRegexPattern"/>.
        /// </param>
        public Numeric3Attribute(bool strict = true) : base(strict ? StrictRegexPattern : RelaxedRegexPattern)
        {
        }
    }
}