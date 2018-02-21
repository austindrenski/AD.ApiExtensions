using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    /// <summary>
    /// Extenion methods to sort an <see cref="IQueryable{T}"/> by property name.
    /// </summary>
    [PublicAPI]
    public static class SortByExtensions
    {
        /// <summary>
        /// The method info for <see cref="Queryable.OrderBy{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})"/>.
        /// </summary>
        [NotNull] private static readonly MethodInfo OrderByMethodInfo;

        /// <summary>
        /// The method info for <see cref="Queryable.OrderByDescending{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})"/>.
        /// </summary>
        [NotNull] private static readonly MethodInfo OrderByDescendingMethodInfo;

        /// <summary>
        /// The method info for <see cref="Queryable.ThenBy{TSource, TKey}(IOrderedQueryable{TSource}, Expression{Func{TSource, TKey}})"/>.
        /// </summary>
        [NotNull] private static readonly MethodInfo ThenByMethodInfo;

        /// <summary>
        /// The method info for <see cref="Queryable.ThenByDescending{TSource, TKey}(IOrderedQueryable{TSource}, Expression{Func{TSource, TKey}})"/>.
        /// </summary>
        [NotNull] private static readonly MethodInfo ThenByDescendingMethodInfo;

        /// <summary>
        /// Caches method info for the later use.
        /// </summary>
        static SortByExtensions()
        {
            OrderByMethodInfo =
                new Func<IQueryable<object>, Expression<Func<object, object>>, IOrderedQueryable<object>>(Queryable.OrderBy).GetMethodInfo().GetGenericMethodDefinition();

            OrderByDescendingMethodInfo =
                new Func<IQueryable<object>, Expression<Func<object, object>>, IOrderedQueryable<object>>(Queryable.OrderByDescending).GetMethodInfo().GetGenericMethodDefinition();

            ThenByMethodInfo =
                new Func<IOrderedQueryable<object>, Expression<Func<object, object>>, IOrderedQueryable<object>>(Queryable.ThenBy).GetMethodInfo().GetGenericMethodDefinition();

            ThenByDescendingMethodInfo =
                new Func<IOrderedQueryable<object>, Expression<Func<object, object>>, IOrderedQueryable<object>>(Queryable.ThenByDescending).GetMethodInfo().GetGenericMethodDefinition();
        }

        /// <summary>
        /// Sorts the <see cref="IQueryable{T}"/> by the <paramref name="keys"/> provided in "[property name]-[sort direction]" form.
        /// E.g. <paramref name="keys"/> = new StringValues { value-asc, year-desc }.
        /// </summary>
        [Pure]
        [NotNull]
        [ItemNotNull]
        public static IOrderedQueryable<T> SortBy<T>([NotNull] [ItemNotNull] this IQueryable<T> source, [NotNull] [ItemCanBeNull] IEnumerable<string> keys)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keys is null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            string[] keysArray = keys.Select(x => x?.ToLower()).ToArray();

            if (keysArray.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException($"The array of sort keys contains a null value: {{ {string.Join(", ", keysArray)} }}");
            }
            
            return
                keysArray.Length.Equals(0)
                    ? source as IOrderedQueryable<T> ?? source.OrderBy(x => x)
                    : keysArray.Length.Equals(1)
                        ? source.OrderByExpression(keysArray[0])
                        : keysArray.Skip(1)
                                   .Aggregate(
                                       source.OrderByExpression(keysArray[0]),
                                       (current, next) =>
                                           current.ThenByExpression(next));
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        private static IOrderedQueryable<T> OrderByExpression<T>([NotNull] [ItemNotNull] this IQueryable<T> source, [NotNull] string key)
        {
            return
                source.Sort(
                    key,
                    key.Contains("-asc")
                        ? OrderByMethodInfo
                        : OrderByDescendingMethodInfo);
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        private static IOrderedQueryable<T> ThenByExpression<T>([NotNull] [ItemNotNull] this IOrderedQueryable<T> source, [NotNull] string key)
        {
            return
                source.Sort(
                    key,
                    key.Contains("-asc")
                        ? ThenByMethodInfo
                        : ThenByDescendingMethodInfo);
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        private static IOrderedQueryable<T> Sort<T>([NotNull] [ItemNotNull] this IQueryable<T> source, [NotNull] string key, [NotNull] MethodInfo method)
        {
            string keyName =
                key.Split('-')
                   .FirstOrDefault();

            PropertyInfo propertyInfo =
                source.ElementType
                      .GetProperties()
                      .SingleOrDefault(x => x.Name.Equals(keyName, StringComparison.OrdinalIgnoreCase));

            if (propertyInfo is null)
            {
                return source as IOrderedQueryable<T> ?? source.OrderBy(x => x);
            }

            MethodInfo methodInfo =
                method.MakeGenericMethod(
                    source.ElementType, 
                    propertyInfo.PropertyType);

            ParameterExpression parameterExpression =
                Expression.Parameter(
                    source.ElementType);

            MethodCallExpression sort =
                Expression.Call(
                    methodInfo,
                    source.Expression,
                    Expression.Quote(
                        Expression.Lambda(
                            Expression.Property(
                                parameterExpression,
                                propertyInfo),
                            parameterExpression)));

            return (IOrderedQueryable<T>) source.Provider.CreateQuery<T>(sort);
        }
    }
}