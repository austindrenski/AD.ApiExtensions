using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AD.ApiExtensions.ModelBinders;
using AD.ApiExtensions.TypeConverters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AD.ApiExtensions.Primitives
{
    /// <inheritdoc cref="IEnumerable{T}" />
    /// <inheritdoc cref="IEquatable{T}" />
    /// <summary>
    /// Represents zero, one, or many <see cref="T:ApiLibrary.Grouping{TKey, TValue}" /> in an efficient way.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the keys by which the values are grouped.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the values in the groups.
    /// </typeparam>
    [PublicAPI]
    [ModelBinder(typeof(GroupingValuesModelBinder))]
    [TypeConverter(typeof(GroupingValuesTypeConverter))]
    public struct GroupingValues<TKey, TValue>
        : IExpressive<TValue>,
          IEnumerable<IGrouping<TKey, TValue>>,
          IEquatable<GroupingValues<TKey, TValue>>,
          IEquatable<IEnumerable<IGrouping<TKey, TValue>>>,
          IEquatable<IGrouping<TKey, TValue>>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// The default zero-length grouping array.
        /// </summary>
        [NotNull] private static readonly Grouping<TKey, TValue>[] DefaultGroupingArray;

        /// <summary>
        /// The default zero-length value array.
        /// </summary>
        [NotNull] private static readonly TValue[] DefaultValueArray;

        /// <summary>
        /// The grouping if singular; otherwise default.
        /// </summary>
        private readonly Grouping<TKey, TValue> _grouping;

        /// <summary>
        /// The groupings if non-singular; otherwise null.
        /// </summary>
        [CanBeNull] private readonly Grouping<TKey, TValue>[] _groupings;

        /// <summary>
        /// True if the groupings contain an empty group that is not a group of individuals; otherwise false.
        /// </summary>
        public bool HasAccumulator => _groupings?.Any(x => x.IsAccumulator) ?? _grouping.IsAccumulator;

        /// <summary>
        /// True if the collection contains values; otherwise false.
        /// </summary>
        public bool HasAny => _groupings?.Any(x => !x.IsEmpty) ?? !_grouping.IsEmpty;

        /// <summary>
        /// True if the groupings contain a group that is empty; otherwise false.
        /// </summary>
        public bool HasEmpty => _groupings?.Any(x => x.IsEmpty) ?? _grouping.IsEmpty;

        /// <summary>
        /// True if the collection contains a key that has a special signature indicating a group of individuals; otherwise false.
        /// </summary>
        public bool HasIndividuals => _groupings?.Any(x => x.IsIndividuals) ?? _grouping.IsIndividuals;

        /// <summary>
        /// True if the collection contains values, an accumulator, or both. Equivalent to <see cref="HasAny"/> || <see cref="HasAccumulator"/>.
        /// </summary>
        public bool IsActive => HasAny || HasAccumulator;

        /// <summary>
        /// True if the collection contains no values, an accumulator, or both. Equivalent to !<see cref="HasAny"/> || <see cref="HasAccumulator"/>.
        /// </summary>
        public bool IsAll => !HasAny || HasAccumulator;

        /// <summary>
        /// True if all of the groupings are empty; otherwise false.
        /// </summary>
        public bool IsEmpty => _groupings?.All(x => x.IsEmpty && !x.IsAccumulator) ?? _grouping.IsEmpty && !_grouping.IsAccumulator;

        /// <summary>
        /// True if the collection contains no values and no accumulator. Equivalent to !<see cref="HasAny"/> &amp;&amp; !<see cref="HasAccumulator"/>.
        /// </summary>
        public bool IsInactive => !HasAny && !HasAccumulator;

        /// <summary>
        /// The accumulator if present; otherwise the default value.
        /// </summary>
        public Grouping<TKey, TValue> Accumulator => _groupings?.SingleOrDefault(x => x.IsAccumulator) ?? (_grouping.IsAccumulator ? _grouping : default);

        /// <summary>
        /// The group of individuals if present; otherwise the default value.
        /// </summary>
        public Grouping<TKey, TValue> Individuals => _groupings?.SingleOrDefault(x => x.IsIndividuals) ?? (_grouping.IsIndividuals ? _grouping : default);

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static GroupingValues()
        {
            DefaultGroupingArray = new Grouping<TKey, TValue>[0];
            DefaultValueArray = new TValue[0];
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> from a key and a value array.
        /// </summary>
        /// <param name="key">
        /// The grouping key.
        /// </param>
        /// <param name="values">
        /// The grouping values.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public GroupingValues([NotNull] TKey key, [NotNull] [ItemNotNull] IEnumerable<TValue> values)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            _grouping = new Grouping<TKey, TValue>(key, values);
            _groupings = DefaultGroupingArray;
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> from a key and a value array.
        /// </summary>
        /// <param name="key">
        /// The grouping key.
        /// </param>
        /// <param name="values">
        /// The grouping values.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public GroupingValues([NotNull] TKey key, [NotNull] [ItemNotNull] params TValue[] values)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            _grouping = new Grouping<TKey, TValue>(key, values);
            _groupings = DefaultGroupingArray;
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> from an <see cref="T:System.Collections.Generic.IEnumerable{T}" /> of <see cref="T:ApiLibrary.Grouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="grouping">
        /// The groupings to store.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public GroupingValues([NotNull] IEnumerable<Grouping<TKey, TValue>> grouping)
        {
            if (grouping is null)
            {
                throw new ArgumentNullException(nameof(grouping));
            }

            _grouping = default;
            _groupings = grouping.Where(x => x != default).ToArray();
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> from an <see cref="T:System.Linq.IGrouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="grouping">
        /// The grouping to store.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public GroupingValues(Grouping<TKey, TValue> grouping)
        {
            _grouping = grouping;
            _groupings = DefaultGroupingArray;
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> from an <see cref="T:System.Linq.IGrouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="grouping">
        /// The grouping to store.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public GroupingValues([NotNull] IGrouping<TKey, TValue> grouping)
        {
            if (grouping is null)
            {
                throw new ArgumentNullException(nameof(grouping));
            }

            _grouping = new Grouping<TKey, TValue>(grouping);
            _groupings = DefaultGroupingArray;
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> from an <see cref="T:System.Collections.Generic.IEnumerable{T}" /> of <see cref="T:System.Linq.IGrouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="grouping">
        /// The groupings to store.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public GroupingValues([NotNull] [ItemNotNull] IEnumerable<IGrouping<TKey, TValue>> grouping)
        {
            if (grouping is null)
            {
                throw new ArgumentNullException(nameof(grouping));
            }

            _grouping = default;
            _groupings =
                (grouping as IEnumerable<Grouping<TKey, TValue>>)?.Where(x => x != default).ToArray()
                ??
                grouping.Select(x => new Grouping<TKey, TValue>(x.Key, x)).ToArray();
        }

        /// <summary>
        /// Constructs a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> from an array of <see cref="T:System.Linq.IGrouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="groupings">
        /// The groupings to store.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException" />
        public GroupingValues([NotNull] [ItemNotNull] params IGrouping<TKey, TValue>[] groupings)
        {
            if (groupings is null)
            {
                throw new ArgumentNullException(nameof(groupings));
            }

            _grouping = default;
            _groupings =
                (groupings as Grouping<TKey, TValue>[])?.Where(x => x != default).ToArray()
                ??
                groupings.Select(x => new Grouping<TKey, TValue>(x.Key, x)).ToArray();
        }

        /// <inheritdoc />
        [Pure]
        public TValue Express(Expression<Func<TValue>> argument)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        [Pure]
        public object Express(Expression argument)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        [Pure]
        public Expression Reduce(Expression<Func<TValue>> argument)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            return new GroupsExpression(argument, this).Reduce();
        }

        /// <inheritdoc />
        [Pure]
        Expression IExpressive.Reduce(Expression argument)
        {
            switch (argument)
            {
                case Expression<Func<TValue>> lambda:
                {
                    return new GroupsExpression(lambda, this).Reduce();
                }
                case UnaryExpression unary:
                {
                    return ((IExpressive) this).Reduce(unary.Operand);
                }
                default:
                {
                    throw new ArgumentException(nameof(argument));
                }
            }
        }

        /// <summary>
        /// True if the collection contains the value; otherwise false.
        /// </summary>
        /// <param name="value">
        /// The value to test.
        /// </param>
        /// <returns>
        /// True if the collection contains the value; otherwise false.
        /// </returns>
        [Pure]
        public bool Contains([NotNull] TValue value)
        {
            return _groupings?.Any(x => x.Contains(value)) ?? _grouping.Contains(value);
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
            return _groupings?.SelectMany(x => x).ToArray() ?? _grouping.ToArray();
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        public override string ToString()
        {
            return _groupings is null ? _grouping.ToString() : string.Join(",", _groupings);
        }

        /// <summary>
        /// Processes the values into groups and items.
        /// </summary>
        /// <param name="values">
        /// The values to process.
        /// </param>
        /// <returns>
        /// The values processed into groups and items.
        /// </returns>
        [Pure]
        public static GroupingValues<string, string> Parse(StringValues values)
        {
            HashSet<Grouping<string, string>> groups = new HashSet<Grouping<string, string>>();
            HashSet<string> individuals = new HashSet<string>();

            foreach (string value in values.SelectMany(x => x.Split(',')))
            {
                if (Grouping<TKey, TValue>.TryParse(value, out Grouping<string, string> result))
                {
                    groups.Add(result);
                }
                else
                {
                    individuals.Add(value);
                }
            }

            StringValues items = individuals.Except(groups.SelectMany(x => x)).ToArray();

            if (items.Any())
            {
                groups.Add(Grouping<string, string>.CreateIndividuals(items));
            }

            return new GroupingValues<string, string>(groups);
        }

        /// <summary>
        /// Casts an <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> to a <see cref="T:ApiLibrary.Grouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="groupingValues">
        /// The grouping values to cast.
        /// </param>
        /// <returns>
        /// The contained singular grouping.
        /// </returns>
        /// <exception cref="T:System.InvalidCastException" />
        [Pure]
        public static explicit operator Grouping<TKey, TValue>(GroupingValues<TKey, TValue> groupingValues)
        {
            if (groupingValues._grouping == default)
            {
                throw new InvalidCastException($"{nameof(groupingValues)} contains more than one grouping.");
            }

            return groupingValues._grouping;
        }

        /// <summary>
        /// Casts an <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" /> to an array of <see cref="T:ApiLibrary.Grouping{TKey, TValue}" />.
        /// </summary>
        /// <param name="groupingValues">
        /// The grouping values to cast.
        /// </param>
        /// <returns>
        /// The contained groupings.
        /// </returns>
        /// <exception cref="T:System.InvalidCastException" />
        [Pure]
        public static implicit operator Grouping<TKey, TValue>[](GroupingValues<TKey, TValue> groupingValues)
        {
            if (groupingValues._grouping != default)
            {
                throw new InvalidCastException($"{nameof(groupingValues)} contains more than one grouping.");
            }

            return groupingValues._groupings?.ToArray() ?? DefaultGroupingArray;
        }

        /// <summary>
        /// Casts an array of <see cref="T:ApiLibrary.Grouping{TKey, TValue}" /> to a <see cref="T:ApiLibrary.GroupingValues{TKey, TValue}" />.
        /// </summary>
        /// <param name="collection">
        /// The collection to cast.
        /// </param>
        /// <returns>
        /// The contained groupings.
        /// </returns>
        /// <exception cref="T:System.InvalidCastException" />
        [Pure]
        public static implicit operator GroupingValues<TKey, TValue>(Grouping<TKey, TValue>[] collection)
        {
            if (collection is null)
            {
                throw new InvalidCastException(nameof(collection));
            }

            return new GroupingValues<TKey, TValue>(collection);
        }

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 397 * _grouping.GetHashCode();

                if (_groupings is null)
                {
                    return hash;
                }

                for (int i = 0; i < _groupings.Length; i++)
                {
                    hash ^= 397 * _groupings[i].GetHashCode();
                }

                return hash;
            }
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(GroupingValues<TKey, TValue> other)
        {
            return this.SequenceEqual(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(IEnumerable<IGrouping<TKey, TValue>> other)
        {
            return this.SequenceEqual(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals([CanBeNull] IGrouping<TKey, TValue> other)
        {
            return !ReferenceEquals(null, other) && (_grouping.Equals(other) || _groupings?.Length == 1 && _groupings[0].Equals(other));
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals([CanBeNull] object obj)
        {
            return !ReferenceEquals(null, obj) && obj is GroupingValues<TKey, TValue> values && Equals(values);
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
        public static bool operator ==(GroupingValues<TKey, TValue> left, GroupingValues<TKey, TValue> right)
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
        public static bool operator !=(GroupingValues<TKey, TValue> left, GroupingValues<TKey, TValue> right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        IEnumerator<IGrouping<TKey, TValue>> IEnumerable<IGrouping<TKey, TValue>>.GetEnumerator()
        {
            if (_grouping != default)
            {
                yield return _grouping;

                yield break;
            }

            if (_groupings is null)
            {
                yield break;
            }

            for (int i = 0; i < _groupings.Length; i++)
            {
                yield return _groupings[i];
            }
        }

        /// <inheritdoc />
        [Pure]
        [NotNull]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IGrouping<TKey, TValue>>).GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents an expression tree that reduces to a series of conditionals assigning group membership.
        /// </summary>
        private sealed class GroupsExpression : Expression
        {
            /// <summary>
            /// The method information for the constructed generic Enumerable.Contains{T}(T value) method.
            /// </summary>
            [NotNull] private static readonly MethodInfo ContainsMethodInfo;

            /// <summary>
            /// The expression associated with this group.
            /// </summary>
            [NotNull] private readonly Expression _expression;

            /// <inheritdoc />
            public override bool CanReduce => false;

            /// <inheritdoc />
            public override ExpressionType NodeType => ExpressionType.Conditional;

            /// <inheritdoc />
            [NotNull]
            public override Type Type => typeof(TValue);

            /// <inheritdoc />
            /// <summary>
            /// Initializes static resources.
            /// </summary>
            static GroupsExpression()
            {
                ContainsMethodInfo =
                    new Func<IEnumerable<object>, object, bool>(Enumerable.Contains)
                        .GetMethodInfo()
                        .GetGenericMethodDefinition()
                        .MakeGenericMethod(typeof(TValue));
            }

            /// <inheritdoc />
            /// <summary>
            /// Constructs an expression from the groupings.
            /// </summary>
            /// <param name="expression">
            /// The default expression.
            /// </param>
            /// <param name="groups">
            /// The groupings.
            /// </param>
            /// <exception cref="T:System.ArgumentNullException" />
            /// <exception cref="T:System.InvalidOperationException" />
            [PublicAPI]
            internal GroupsExpression([NotNull] Expression<Func<TValue>> expression, IEnumerable<IGrouping<TKey, TValue>> groups)
            {
                if (expression is null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }

                GroupingValues<TKey, TValue> validGroups =
                    groups.AsGrouping()
                          .Where(x => x != default)
                          .ToArray();

                if (!validGroups.IsActive)
                {
                    _expression = expression.Body;
                    return;
                }

                // Define the default expression to be either an accumulator or provided the expression body.
                Expression defaultExpression =
                    validGroups.HasAccumulator
                        ? Constant(validGroups.Accumulator.Key, typeof(TKey))
                        : expression.Body;

                Stack<Expression> conditions = new Stack<Expression>();

                // If the individuals group is present and not empty, push it onto the stack.
                if (!validGroups.Individuals.IsEmpty)
                {
                    Expression conditional =
                        Condition(
                            Call(ContainsMethodInfo, Constant(validGroups.Individuals, typeof(IGrouping<TKey, TValue>)), expression.Body),
                            expression.Body,
                            defaultExpression);

                    conditions.Push(conditional);
                }

                // If the stack is empty, take the default expression; otherwise pop the current item. Push the new item onto the stack.
                foreach (Grouping<TKey, TValue> group in validGroups.AsGrouping().Where(x => x.IsCommon))
                {
                    Expression conditional =
                        Condition(
                            Call(ContainsMethodInfo, Constant(group, typeof(IGrouping<TKey, TValue>)), expression.Body),
                            Constant(group.Key, typeof(TKey)),
                            conditions.Any() ? conditions.Pop() : defaultExpression);

                    conditions.Push(conditional);
                }

                // If the stack is empty, take the default expression; otherwise pop the current item.
                _expression = conditions.Any() ? conditions.Pop() : defaultExpression;

                if (conditions.Any())
                {
                    throw new InvalidOperationException($"Failed to completely aggregate to the {nameof(GroupsExpression)}.");
                }
            }

            /// <inheritdoc />
            [Pure]
            [NotNull]
            public override Expression Reduce()
            {
                return _expression;
            }
        }
    }
}