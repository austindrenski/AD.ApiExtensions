﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Primitives
{
    /// <inheritdoc cref="IGrouping{TKey, TValue}" />
    /// <inheritdoc cref="IEquatable{T}" />
    /// <summary>
    /// Represents an <see cref="T:System.Linq.IGrouping{TKey, TValue}" /> as a struct with <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" />.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the keys by which the values are grouped.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the values in the groups. 
    /// </typeparam>
    [PublicAPI]
    public struct Grouping<TKey, TValue>
        : IGrouping<TKey, TValue>,
          IEquatable<Grouping<TKey, TValue>>,
          IEquatable<IGrouping<TKey, TValue>>
    {
        /// <summary>
        /// The regular expression string to match strings in the form: NAME(MEMBER-MEMBER-MEMBER).
        /// </summary>
        [NotNull] private const string NameMembersRegexString = "(?<Key>[A-z0-9_]+)\\((?<Members>[A-z0-9\\-]*)\\)";

        /// <summary>
        /// The regular expression string to match strings in the form: MEMBER-MEMBER-MEMBER.
        /// </summary>
        [NotNull] private const string MembersRegexString = "(?<Member>[A-z0-9]+)";

        /// <summary>
        /// The name of the individuals group.
        /// </summary>
        [NotNull] private const string IndividualsGroupName = "<>__individuals";

        /// <summary>
        /// The regex applied to match the name and members of a group.
        /// </summary>
        [NotNull] private static readonly Regex OuterRegex;

        /// <summary>
        /// The regex applied to match the members of group.
        /// </summary>
        [NotNull] private static readonly Regex InnerRegex;

        /// <summary>
        /// The default zero-length values array. 
        /// </summary>
        [NotNull] private static readonly TValue[] DefaultValuesArray;

        /// <summary>
        /// The values array or null.
        /// </summary>
        [CanBeNull] private readonly TValue[] _values;

        /// <inheritdoc />
        public TKey Key { get; }

        /// <summary>
        /// True if the grouping is an empty group that is not a group of individuals; otherwise false.
        /// </summary>
        public bool IsAccumulator => this != default && IsEmpty && !IsIndividuals;

        /// <summary>
        /// True if the grouping is neither an empty group nor a group of individuals; otherwise false.
        /// </summary>
        public bool IsCommon => !IsEmpty && !IsIndividuals;

        /// <summary>
        /// True if the group is empty; otherwise false.
        /// </summary>
        public bool IsEmpty => _values is null || _values.Length == 0;

        /// <summary>
        /// True if the key contains a special signature indicating a group of individuals; otherwise false.
        /// </summary>
        public bool IsIndividuals => IsIndividualsGroup(Key);

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static Grouping()
        {
            DefaultValuesArray = new TValue[0];
            OuterRegex = new Regex(NameMembersRegexString, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            InnerRegex = new Regex(MembersRegexString, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.Grouping{TKey, TValue}" /> from a key and a value array.
        /// </summary>
        /// <param name="key">
        /// The grouping key.
        /// </param>
        /// <param name="values">
        /// The grouping values.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public Grouping([NotNull] TKey key, [NotNull] IEnumerable<TValue> values)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Key = key;
            _values = values.ToArray();
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.Grouping{TKey, TValue}" /> from a key and a value array.
        /// </summary>
        /// <param name="key">
        /// The grouping key.
        /// </param>
        /// <param name="values">
        /// The grouping values.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public Grouping([NotNull] TKey key, [NotNull] params TValue[] values)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Key = key;
            _values = values.ToArray();
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.Grouping{TKey, TValue}" /> from an <see cref="T:System.Linq.IGrouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="grouping">
        /// The grouping to store.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public Grouping([NotNull] IGrouping<TKey, TValue> grouping)
        {
            if (grouping is null)
            {
                throw new ArgumentNullException(nameof(grouping));
            }

            Key = grouping.Key;
            _values = grouping.ToArray();
        }

        /// <summary>
        /// Creates a <see cref="Grouping{TKey, TValue}"/> whose key is set to a special string indicating that the group is a collection of individuals.
        /// </summary>
        /// <param name="items">
        /// The individual items for the individuals collection.
        /// </param>
        /// <returns>
        /// A <see cref="Grouping{TKey, TValue}"/> whose key is set to a special string indicating that the group is a collection of individuals. 
        /// </returns>
        [Pure]
        public static Grouping<string, TValue> CreateIndividuals([NotNull] IEnumerable<TValue> items)
        {
            return new Grouping<string, TValue>(IndividualsGroupName, items);
        }

        /// <summary>
        /// Returns true if the key contains a special signature; otherwise false.
        /// </summary>
        [Pure]
        public static bool IsIndividualsGroup([CanBeNull] TKey key)
        {
            return IndividualsGroupName.Equals(key);
        }

        /// <summary>
        /// Returns the values as an array.
        /// </summary>
        /// <returns>
        /// The values as an array.
        /// </returns>
        [Pure]
        [NotNull]
        public TValue[] ToArray()
        {
            return _values?.ToArray() ?? DefaultValuesArray;
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        public override string ToString()
        {
            return $"{Key}({string.Join("-", _values ?? DefaultValuesArray)})";
        }

        /// <summary>
        /// Parses a grouping from the string.
        /// </summary>
        /// <param name="input">
        /// The string to parse.
        /// </param>
        /// <returns>
        /// A <see cref="T:ApiLibrary.Grouping{TKey, TValue}" /> containing the features.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException" />
        /// <exception cref="T:System.ArgumentException" />
        [Pure]
        public static Grouping<string, string> Parse([NotNull] string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (!OuterRegex.IsMatch(input))
            {
                return new Grouping<string, string>(input, input);
            }

            Match parse = OuterRegex.Match(input);

            string key = parse.Groups["Key"].Value;

            IEnumerable<string> members =
                InnerRegex.Matches(parse.Groups["Members"].Value)
                          .Cast<Match>()
                          .Select(x => x.Groups["Member"].Value);

            return new Grouping<string, string>(key, members);
        }

        /// <summary>
        /// Parses a grouping from the string.
        /// </summary>
        /// <param name="value">
        /// The string to parse.
        /// </param>
        /// <param name="result">
        /// The result value.
        /// </param>
        /// <returns>
        /// True if the value was able to be parsed; otherwise false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException" />
        /// <exception cref="T:System.ArgumentException" />
        [Pure]
        public static bool TryParse([NotNull] string value, out Grouping<string, string> result)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!OuterRegex.IsMatch(value))
            {
                result = default;
                return false;
            }

            Match parse = OuterRegex.Match(value);

            string key = parse.Groups["Key"].Value;

            IEnumerable<string> members =
                InnerRegex.Matches(parse.Groups["Members"].Value)
                          .Cast<Match>()
                          .Select(x => x.Groups["Member"].Value);

            result = new Grouping<string, string>(key, members);
            return true;
        }

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode()
        {
            unchecked
            {
                if (Key == null)
                {
                    return 0;
                }

                int hash = 397 * Key.GetHashCode();

                if (_values is null)
                {
                    return hash;
                }

                for (int i = 0; i < _values.Length; i++)
                {
                    hash ^= 397 * _values[i].GetHashCode();
                }

                return hash;
            }
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(Grouping<TKey, TValue> other)
        {
            if (_values is null && other._values is null)
            {
                return true;
            }

            return Key.Equals(other.Key) && this.SequenceEqual(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals([CanBeNull] IGrouping<TKey, TValue> other)
        {
            return other != null && Key.Equals(other.Key) && this.SequenceEqual(other);
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals([CanBeNull] object obj)
        {
            return !ReferenceEquals(null, obj) && obj is Grouping<TKey, TValue> values && Equals(values);
        }

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">
        /// The left value to compare.
        /// </param>
        /// <param name="right">
        /// The right value to compare.
        /// </param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator ==(Grouping<TKey, TValue> left, Grouping<TKey, TValue> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">
        /// The left value to compare.
        /// </param>
        /// <param name="right">
        /// The right value to compare.
        /// </param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator !=(Grouping<TKey, TValue> left, Grouping<TKey, TValue> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">
        /// The left value to compare.
        /// </param>
        /// <param name="right">
        /// The right value to compare.
        /// </param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator ==(Grouping<TKey, TValue> left, [NotNull] IGrouping<TKey, TValue> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">
        /// The left value to compare.
        /// </param>
        /// <param name="right">
        /// The right value to compare.
        /// </param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator !=(Grouping<TKey, TValue> left, [NotNull] IGrouping<TKey, TValue> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">
        /// The left value to compare.
        /// </param>
        /// <param name="right">
        /// The right value to compare.
        /// </param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator ==([NotNull] IGrouping<TKey, TValue> left, Grouping<TKey, TValue> right)
        {
            return right.Equals(left);
        }

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">
        /// The left value to compare.
        /// </param>
        /// <param name="right">
        /// The right value to compare.
        /// </param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator !=([NotNull] IGrouping<TKey, TValue> left, Grouping<TKey, TValue> right)
        {
            return !right.Equals(left);
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            if (_values is null)
            {
                yield break;
            }

            for (int i = 0; i < _values.Length; i++)
            {
                yield return _values[i];
            }
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValue>) this).GetEnumerator();
        }
    }
}