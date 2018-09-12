using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    // TODO: document TypeCache
    /// <summary>
    /// Represents a cache of types and updates.
    /// </summary>
    [PublicAPI]
    public class TypeCache
    {
        /// <summary>
        /// Caches anonymous types that were encountered and updated.
        /// </summary>
        [NotNull] private readonly IDictionary<Type, Type> _types;

        /// <summary>
        ///
        /// </summary>
        [NotNull] private readonly ISet<(string Name, Type Type)> _differences;

        /// <summary>
        /// Caches anonymous types that were encountered and modified.
        /// </summary>
        [NotNull] private readonly IDictionary<Type, ParameterExpression> _parameters;

        /// <summary>
        /// True if the cache is empty; otherwise false;
        /// </summary>
        public bool IsEmpty => !_types.Any();

        /// <summary>
        ///
        /// </summary>
        public TypeCache()
        {
            _types = new Dictionary<Type, Type>();
            _differences = new HashSet<(string Name, Type Type)>();
            _parameters = new Dictionary<Type, ParameterExpression>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        public bool ContainsKey([NotNull] Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return _types.ContainsKey(type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="methodInfo">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public MethodInfo GetMethodInfoOrInput([NotNull] MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            return
                !methodInfo.IsGenericMethod
                    ? methodInfo
                    : methodInfo.GetGenericMethodDefinition()
                                .MakeGenericMethod(
                                    methodInfo.GetGenericArguments()
                                              .Select(GetTypeOrInput)
                                              .ToArray());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public Expression GetParameterOrInput([NotNull] Expression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return node is ParameterExpression && _parameters.TryGetValue(node.Type, out ParameterExpression result) ? result : node;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public Type GetTypeOrInput([NotNull] Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return _types.TryGetValue(type, out Type result) ? result : type;
        }

        /// <summary>
        /// Tests if it is provable that an assignment results in a default initialization.
        /// </summary>
        /// <param name="expression">
        /// The expression for assignment.
        /// </param>
        /// <param name="unavailable">
        /// The property names that were explicitly removed by the time of testing.
        /// </param>
        /// <param name="memberInfo">
        /// The target of assignment.
        /// </param>
        /// <returns>
        /// True if it is provable that the assignment results in a default initialization; otherwise false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException" />
        [Pure]
        public bool IsLogicallyDefault(
            [NotNull] MemberInfo memberInfo,
            [NotNull] Expression expression,
            [NotNull] IDictionary<string, MemberInfo> unavailable)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (unavailable == null)
            {
                throw new ArgumentNullException(nameof(unavailable));
            }

            if (expression is ConstantExpression constantExpression)
            {
                return IsDefault(constantExpression.Value);
            }

            if (IsEmpty || IsUpdatedMemberName(memberInfo.Name))
            {
                return false;
            }

//            // TODO: look into this to replace the current "unavailable" methodology.
//                return _differences.Contains((memberInfo.Name, memberInfo.GetType()));
            return unavailable.ContainsKey(memberInfo.Name) && unavailable[memberInfo.Name].GetType() == memberInfo.GetType();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        private bool IsUpdatedMemberName([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return
                _types.Select(x => x.Value)
                      .SelectMany(x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                      .Select(y => y.Name)
                      .Contains(name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type">
        ///
        /// </param>
        /// <param name="result">
        ///
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public void Register([NotNull] Type type, [NotNull] Type result)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!type.IsConstructedGenericType)
            {
                return;
            }

            if (_types.ContainsKey(type))
            {
                return;
            }

            Type recurseType = RecurseType(result);

            ParameterExpression parameter =
                _parameters.Values.FirstOrDefault(x => x.Type == recurseType)
                ?? Expression.Parameter(recurseType, $"param_{recurseType.Name}");

            _types.Add(type, parameter.Type);
            _parameters.Add(type, parameter);

//            // TODO: look into this to replace the current "unavailable" methodology.
//            IEnumerable<(string Name, Type PropertyType)> keys =
//                _types.Select(x => x.Key)
//                      .SelectMany(x => x.GetProperties(BindingFlags.FlattenHierarchy).Select(y => (y.Name, y.PropertyType)))
//                      .Distinct();
//
//            IEnumerable<(string Name, Type PropertyType)> values =
//                _types.Select(x => x.Value)
//                      .SelectMany(x => x.GetProperties(BindingFlags.FlattenHierarchy).Select(y => (y.Name, y.PropertyType)))
//                      .Distinct();
//
//            foreach ((string Name, Type PropertyType) d in keys.Except(values))
//            {
//                _differences.Add(d);
//            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type">
        ///
        /// </param>
        /// <param name="result"
        ///
        /// ></param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [
            Pure]
        public bool TryGetParameter([NotNull] Type type, [CanBeNull] out ParameterExpression result)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return _parameters.TryGetValue(type, out result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        private Type RecurseType([NotNull] Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return
                !type.IsConstructedGenericType
                    ? type
                    : GetTypeOrInput(
                        type.GetGenericTypeDefinition()
                            .MakeGenericType(
                                type.GenericTypeArguments
                                    .Select(RecurseType)
                                    .ToArray()));
        }

        /// <summary>
        /// Tests if the item is recognized as the default value of the underlying type.
        /// </summary>
        /// <param name="item">
        /// The item to test.
        /// </param>
        /// <returns>
        /// True if the item is recognized as the default value; otherwise false.
        /// </returns>
        private static bool IsDefault([CanBeNull] object item)
        {
            switch (item)
            {
                case null:
                {
                    return true;
                }
                case char value:
                {
                    return value is '\0';
                }
                case bool value:
                {
                    return value is false;
                }
                case byte value:
                {
                    return value is (byte) 0;
                }
                case sbyte value:
                {
                    return value is (sbyte) 0;
                }
                case decimal value:
                {
                    return value is (decimal) 0;
                }
                case double value:
                {
                    return value is (double) 0;
                }
                case float value:
                {
                    return value is (float) 0;
                }
                case int value:
                {
                    return value is 0;
                }
                case uint value:
                {
                    return value is (uint) 0;
                }
                case long value:
                {
                    return value is (long) 0;
                }
                case ulong value:
                {
                    return value is (ulong) 0;
                }
                case short value:
                {
                    return value is (short) 0;
                }
                case ushort value:
                {
                    return value is (ushort) 0;
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}