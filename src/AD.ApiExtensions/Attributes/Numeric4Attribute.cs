using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Validates that a property contains three numeric characters.
    /// </summary>
    [PublicAPI]
    public class Numeric4Attribute : RegularExpressionAttribute
    {
        /// <summary>
        /// A pattern of 4 numeric characters.
        /// </summary>
        public const string RegexPattern = "^[0-9]{4}$";

        /// <inheritdoc />
        /// <summary>
        /// Validates that a property contains four numeric characters.
        /// </summary>
        public Numeric4Attribute() : base(RegexPattern)
        {
        }
    }
}