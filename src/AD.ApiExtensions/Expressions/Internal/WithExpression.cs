using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions.Internal
{
    /// <summary>
    /// Represents an expression that rebinds a specified property or field.
    /// </summary>
    [PublicAPI]
    public class WithExpression<TSource, TValue> : Expression, IEquatable<WithExpression<TSource, TValue>>
    {
        #region Resources

        /// <summary>
        /// The static generic <see cref="MethodInfo"/> for Queryable.Select{TIn, TOut}(...).
        /// </summary>
        [NotNull] static readonly MethodInfo SelectMethodInfo =
            new Func<IQueryable<object>, Expression<Func<object, object>>, IQueryable<object>>(Queryable.Select)
                .GetMethodInfo()
                .GetGenericMethodDefinition();

        /// <summary>
        /// The cache of non-generic select methods.
        /// </summary>
        [NotNull] static readonly ConcurrentDictionary<Type, MethodInfo> SelectorCache =
            new ConcurrentDictionary<Type, MethodInfo>();

        #endregion

        /// <inheritdoc />
        public override bool CanReduce => true;

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type => Source.Type;

        /// <summary>
        /// The source expression.
        /// </summary>
        [NotNull]
        public Expression Source { get; }

        /// <summary>
        /// The member to bind.
        /// </summary>
        [NotNull]
        public MemberExpression Target { get; }

        /// <summary>
        /// The value to bind.
        /// </summary>
        [NotNull]
        public Expression<Func<TSource, TValue>> Value { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="WithExpression{TSource,TValue}"/> class.
        /// </summary>
        /// <param name="source">The source expression.</param>
        /// <param name="target">The target to bind.</param>
        /// <param name="value">The value to bind.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="target"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/></exception>
        public WithExpression(
            [NotNull] Expression source,
            [NotNull] Expression<Func<TSource, TValue>> target,
            [NotNull] Expression<Func<TSource, TValue>> value)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (!(target.Body is MemberExpression member) || member.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException($"{nameof(target)} must be a {nameof(MemberExpression)} of {MemberTypes.Property}.");

            Source = source;
            Target = member;
            Value = value;
        }

        /// <inheritdoc />
        [Pure]
        public override Expression Reduce()
        {
            PropertyInfo[] properties =
                typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            (MemberInfo Member, Expression Expression)[] expressions =
                properties.Select(x => Property(Target.Expression, typeof(TSource), x.Name))
                          .Select(x => (x.Member, Expression: x.Member == Target.Member ? Value.Body : x))
                          .ToArray();

            Type[] types =
                properties.Select(x => x.PropertyType)
                          .ToArray();

            Expression body =
                typeof(TSource).GetConstructor(types) is ConstructorInfo m
                    ? (Expression) New(m, expressions.Select(x => x.Expression), expressions.Select(x => x.Member))
                    : MemberInit(New(typeof(TSource)), expressions.Select(x => Bind(x.Member, x.Expression)));

            MethodInfo selector =
                SelectorCache.GetOrAdd(typeof(TSource), t => SelectMethodInfo.MakeGenericMethod(t, body.Type));

            return Call(selector, Source, Lambda(body, Value.Parameters.Single()));
        }

        /// <inheritdoc />
        [Pure]
        public override string ToString() => $"{Source}.With({Target}, {Value})";

        /// <inheritdoc />
        [Pure]
        public bool Equals(WithExpression<TSource, TValue> other)
            => other != null &&
               Source.Equals(other.Source) &&
               Target.Equals(other.Target) &&
               Value.Equals(other.Value);

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is WithExpression<TSource, TValue> w && Equals(w);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Source.GetHashCode();
                hashCode = (397 * hashCode) ^ Target.GetHashCode();
                hashCode = (397 * hashCode) ^ Value.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="WithExpression{TSource,TValue}"/> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// True if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.
        /// </returns>
        [Pure]
        public static bool operator ==(WithExpression<TSource, TValue> left, WithExpression<TSource, TValue> right) => Equals(left, right);

        /// <summary>Returns a value that indicates whether two <see cref="WithExpression{TSource,TValue}" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// True if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.
        /// </returns>
        [Pure]
        public static bool operator !=(WithExpression<TSource, TValue> left, WithExpression<TSource, TValue> right) => !Equals(left, right);
    }
}