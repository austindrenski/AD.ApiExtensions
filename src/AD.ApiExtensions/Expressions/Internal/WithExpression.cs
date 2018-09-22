using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class WithExpression : Expression, IEquatable<WithExpression>
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
        /// The element type of the source expression.
        /// </summary>
        [NotNull]
        public virtual Type ElementType { get; }

        /// <summary>
        /// The source expression.
        /// </summary>
        [NotNull]
        public Expression Source { get; }

        /// <summary>
        /// The member to bind.
        /// </summary>
        [NotNull]
        public MemberExpression Member { get; }

        /// <summary>
        /// The value to bind.
        /// </summary>
        [NotNull]
        public LambdaExpression Value { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="WithExpression"/> class.
        /// </summary>
        /// <param name="elementType">The element type of the source expression.</param>
        /// <param name="source">The source expression.</param>
        /// <param name="member">The member to bind.</param>
        /// <param name="value">The value to bind.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elementType"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="source"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="member"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/></exception>
        public WithExpression(
            [NotNull] Type elementType,
            [NotNull] Expression source,
            [NotNull] MemberExpression member,
            [NotNull] LambdaExpression value)
        {
            if (elementType is null)
                throw new ArgumentNullException(nameof(elementType));

            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (member is null)
                throw new ArgumentNullException(nameof(member));

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            ElementType = elementType;
            Source = source;
            Member = member;
            Value = value;
        }

        /// <inheritdoc />
        [Pure]
        public override Expression Reduce()
        {
            ParameterExpression parameter = Parameter(ElementType);

            Expression rebind = new ParameterRebindingExpressionVisitor(parameter).Visit(Value.Body);

            IReadOnlyList<PropertyInfo> properties =
                parameter.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            (MemberInfo Member, Expression Expression)[] expressions =
                properties.Select(x => Property(parameter, x))
                          .Select(x => (x.Member, Expression: x.Member == Member.Member ? rebind : x))
                          .ToArray();

            Type[] types =
                properties.Select(x => x.PropertyType)
                          .ToArray();

            Expression body =
                parameter.Type.GetConstructor(types) is ConstructorInfo m
                    ? New(m, expressions.Select(x => x.Expression), expressions.Select(x => x.Member))
                    : (Expression) MemberInit(New(parameter.Type), expressions.Select(x => Bind(x.Member, x.Expression)));

            MethodInfo selector =
                SelectorCache.GetOrAdd(parameter.Type, t => SelectMethodInfo.MakeGenericMethod(t, body.Type));

            MethodCallExpression call = Call(selector, Source, Lambda(body, parameter));

            return call;
        }

        /// <inheritdoc />
        [Pure]
        public override string ToString() => $"{Type.Name} => {ElementType.Name}.{Member.Member.Name} = {Value.Body}";

        /// <inheritdoc />
        [Pure]
        public bool Equals(WithExpression other)
            => other != null &&
               Type == other.Type &&
               Source.Equals(other.Source) &&
               Member.Equals(other.Member) &&
               Value.Equals(other.Value);

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is WithExpression w && Equals(w);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ElementType.GetHashCode();
                hashCode = (397 * hashCode) ^ Source.GetHashCode();
                hashCode = (397 * hashCode) ^ Member.GetHashCode();
                hashCode = (397 * hashCode) ^ Value.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="WithExpression"/> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// True if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.
        /// </returns>
        [Pure]
        public static bool operator ==(WithExpression left, WithExpression right) => Equals(left, right);

        /// <summary>Returns a value that indicates whether two <see cref="WithExpression" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// True if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.
        /// </returns>
        [Pure]
        public static bool operator !=(WithExpression left, WithExpression right) => !Equals(left, right);
    }
}