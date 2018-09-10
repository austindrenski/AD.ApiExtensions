using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace AD.ApiExtensions
{
    /// <summary>
    /// A name translator which converts standard CLR names (e.g. SomeClass)
    /// to delimited-case names (e.g. some_class, some-class, some/class).
    /// </summary>
    [PublicAPI]
    public static class StringCaseNameTranslator
    {
        /// <summary>
        /// Converts a string to its snake-case equivalent.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>
        /// The name converted to snake case.
        /// </returns>
        [Pure]
        [CanBeNull]
        public static string ConvertToSnakeCase([CanBeNull] this string name) => ConvertToCase(name, '_');

        /// <summary>
        /// Converts a string to its kebab-case equivalent.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>
        /// The name converted to kebab case.
        /// </returns>
        [Pure]
        [CanBeNull]
        public static string ConvertToKebabCase([CanBeNull] this string name) => ConvertToCase(name, '-');

        /// <summary>
        /// Converts a string to its path-case equivalent.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>
        /// The name converted to path case.
        /// </returns>
        [Pure]
        [CanBeNull]
        public static string ConvertToPathCase([CanBeNull] this string name) => ConvertToCase(name, '/');

        /// <summary>
        /// Converts a string to its delimited-case equivalent.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <param name="delimiter">The delimiting character.</param>
        /// <returns>
        /// The name converted to a delimited case.
        /// </returns>
        /// <remarks>
        /// Code based on Newtonsoft.Json.
        /// See: https://github.com/JamesNK/Newtonsoft.Json/blob/92258c212e8924640f78e0f019127392ec7a054f/Src/Newtonsoft.Json/Utilities/StringUtils.cs#L200-L276
        /// </remarks>
        [Pure]
        [CanBeNull]
        public static string ConvertToCase([CanBeNull] string name, char delimiter)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            StringBuilder sb = new StringBuilder();
            StringCaseState state = StringCaseState.Start;

            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == ' ')
                {
                    if (state != StringCaseState.Start)
                        state = StringCaseState.NewWord;
                }
                else if (char.IsUpper(name[i]))
                {
                    switch (state)
                    {
                        case StringCaseState.Upper:
                            bool hasNext = i + 1 < name.Length;
                            if (i > 0 && hasNext)
                            {
                                char nextChar = name[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != delimiter)
                                    sb.Append(delimiter);
                            }

                            break;

                        case StringCaseState.Lower:
                        case StringCaseState.NewWord:
                            sb.Append(delimiter);
                            break;

                        case StringCaseState.Start:
                        default:
                            break;
                    }

                    sb.Append(char.ToLowerInvariant(name[i]));
                    state = StringCaseState.Upper;
                }
                else if (name[i] == delimiter)
                {
                    sb.Append(delimiter);
                    state = StringCaseState.Start;
                }
                else
                {
                    if (state == StringCaseState.NewWord)
                        sb.Append(delimiter);

                    sb.Append(name[i]);
                    state = StringCaseState.Lower;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Tracks the state of a name translation.
        /// </summary>
        enum StringCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }
    }
}