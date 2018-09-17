using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    /// <summary>
    /// Defines a new anonymous type.
    /// </summary>
    [PublicAPI]
    public readonly struct TypeDefinition : IEquatable<TypeDefinition>
    {
        [NotNull] const string AnonymousAssemblyName = "AD.ApiExtensions.AnonymousTypes";

        [NotNull] static readonly ModuleBuilder ModuleBuilder =
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AnonymousAssemblyName), AssemblyBuilderAccess.Run)
                           .DefineDynamicModule(AnonymousAssemblyName);

        /// <summary>
        /// The previously defined types.
        /// </summary>
        [NotNull] static readonly ConcurrentDictionary<int, Type> Types = new ConcurrentDictionary<int, Type>();

        /// <summary>
        /// Tracks the number of type definitions created.
        /// </summary>
        static long _typeCount;

        /// <summary>
        /// The type defined by the <see cref="TypeDefinition"/>.
        /// </summary>
        [NotNull] readonly TypeInfo _type;

        /// <summary>
        /// Constructs a new <see cref="TypeDefinition"/> instance.
        /// </summary>
        /// <param name="properties">The name-type pairs to construct instance properties.</param>
        /// <exception cref="ArgumentNullException"><paramref name="properties"/></exception>
        TypeDefinition([NotNull] (string Name, Type Type)[] properties)
        {
            TypeBuilder typeBuilder =
                ModuleBuilder.DefineType(
                    $"f__Anonymous__{Interlocked.Increment(ref _typeCount)}",
                    TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.Serializable,
                    typeof(ValueType));

            DefineConstructor(typeBuilder, properties);

            _type = typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Creates a new anonymous type similar to CLR-generated anonymous types.
        /// </summary>
        /// <param name="properties">The property information to include in the new type.</param>
        /// <returns>
        /// A new type that behaves like a CLR-generated anonymous type.
        /// </returns>
        /// <remarks>
        /// The type is named with the pattern: f__Anonymous__{int}.
        /// Unlike CLR-generated anonymous types, the created type is serializable.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="properties"/></exception>
        [NotNull]
        public static Type GetOrAdd([NotNull] IEnumerable<(MemberInfo Member, Expression Expression)> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            return GetOrAdd(properties.Select(x => (x.Member.Name, x.Expression.Type)));
        }

        /// <summary>
        /// Creates a new anonymous type similar to CLR-generated anonymous types.
        /// </summary>
        /// <param name="properties">The property information to include in the new type.</param>
        /// <returns>
        /// A new type that behaves like a CLR-generated anonymous type.
        /// </returns>
        /// <remarks>
        /// The type is named with the pattern: f__Anonymous__{int}.
        /// Unlike CLR-generated anonymous types, the created type is serializable.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="properties"/></exception>
        [NotNull]
        public static Type GetOrAdd([NotNull] IEnumerable<(string Name, Type Type)> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            (string Name, Type Type)[] propertyInfo = properties as (string Name, Type Type)[] ?? properties.ToArray();

            int hashCode = propertyInfo.Aggregate(397, (current, next) => unchecked(current ^ (397 * next.GetHashCode())));

            return Types.GetOrAdd(hashCode, _ => new TypeDefinition(propertyInfo));
        }

        /// <summary>
        /// Casts the <see cref="TypeDefinition"/> to its internal <see cref="TypeInfo"/>.
        /// </summary>
        /// <param name="definition">The <see cref="TypeDefinition"/> to cast.</param>
        /// <returns>
        /// The internal <see cref="TypeInfo"/> of the <see cref="TypeDefinition"/>.
        /// </returns>
        [Pure]
        [NotNull]
        public static implicit operator TypeInfo(TypeDefinition definition) => definition._type;

        /// <inheritdoc />
        public override int GetHashCode() => _type.GetHashCode();

        /// <inheritdoc />
        public bool Equals(TypeDefinition other) => _type == other._type;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is TypeDefinition type && Equals(type);

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">The left value to compare.</param>
        /// <param name="right">The right value to compare.</param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator ==(TypeDefinition left, TypeDefinition right) => left.Equals(right);

        /// <summary>
        /// Compares two values.
        /// </summary>
        /// <param name="left">The left value to compare.</param>
        /// <param name="right">The right value to compare.</param>
        /// <returns>
        /// True if equal; otherwise false.
        /// </returns>
        [Pure]
        public static bool operator !=(TypeDefinition left, TypeDefinition right) => !left.Equals(right);

        /// <summary>
        /// Defines a default constructor and an overloaded constructor that assigns the specified members.
        /// </summary>
        /// <param name="builder">The <see cref="TypeBuilder"/> to mutate.</param>
        /// <param name="properties">The properties to be injected during construction.</param>
        static void DefineConstructor([NotNull] TypeBuilder builder, [NotNull] (string Name, Type Type)[] properties)
        {
            FieldInfo[] fields = new FieldInfo[properties.Length];
            Type[] fieldTypes = new Type[properties.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = new MemberDefinition(properties[i].Name, properties[i].Type, builder);
                fieldTypes[i] = fields[i].FieldType;
            }

            ILGenerator ctorIl =
                builder.DefineConstructor(MethodAttributes.PrivateScope, CallingConventions.Standard, fieldTypes)
                       .GetILGenerator();

            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, builder.DefineDefaultConstructor(MethodAttributes.Public));

            for (int i = 0; i < fields.Length; i++)
            {
                ctorIl.Emit(OpCodes.Ldarg_0);
                ctorIl.Emit(OpCodes.Ldarg, i + 1);
                ctorIl.Emit(OpCodes.Stfld, fields[i]);
            }

            ctorIl.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Private structure to define a public property and a backing field.
        /// </summary>
        private readonly struct MemberDefinition
        {
            /// <summary>
            /// Defines the method attributes needed to generate CLR compliant getter and setters in IL code.
            /// </summary>
            [NotNull] const MethodAttributes GetSetAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;

            /// <summary>
            /// The backing field for the public property.
            /// </summary>
            [NotNull] readonly FieldInfo _fieldInfo;

            /// <summary>
            /// Constructs an instance of the <see cref="MemberDefinition"/> struct.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            /// <param name="propertyType">The type of the property.</param>
            /// <param name="typeBuilder">The type builder to mutate.</param>
            internal MemberDefinition([NotNull] string propertyName, [NotNull] Type propertyType, [NotNull] TypeBuilder typeBuilder)
            {
                _fieldInfo = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, _fieldInfo.FieldType, Type.EmptyTypes);

                DefineGetter(typeBuilder, propertyBuilder, _fieldInfo);
                DefineSetter(typeBuilder, propertyBuilder, _fieldInfo);
            }

            /// <summary>
            /// Casts the <see cref="MemberDefinition"/> to its internal <see cref="FieldInfo"/>.
            /// </summary>
            /// <param name="definition">The <see cref="MemberDefinition"/> to cast.</param>
            /// <returns>
            /// The internal <see cref="FieldInfo"/> of the <see cref="MemberDefinition"/>.
            /// </returns>
            [Pure]
            [NotNull]
            public static implicit operator FieldInfo(MemberDefinition definition) => definition._fieldInfo;

            /// <summary>
            /// Defines the method body the GetMethod of this member definition.
            /// </summary>
            static void DefineGetter(
                [NotNull] TypeBuilder builder,
                [NotNull] PropertyBuilder propertyBuilder,
                [NotNull] FieldInfo field)
            {
                MethodBuilder get = builder.DefineMethod($"get{field.Name}", GetSetAttributes, field.FieldType, Type.EmptyTypes);
                ILGenerator methodIl = get.GetILGenerator();

                methodIl.Emit(OpCodes.Ldarg_0);
                methodIl.Emit(OpCodes.Ldfld, field);
                methodIl.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(get);
            }

            /// <summary>
            /// Defines the method body the SetMethod of this member definition.
            /// </summary>
            static void DefineSetter(
                [NotNull] TypeBuilder builder,
                [NotNull] PropertyBuilder propertyBuilder,
                [NotNull] FieldInfo field)
            {
                MethodBuilder set = builder.DefineMethod($"set{field.Name}", GetSetAttributes, null, new Type[] { field.FieldType });
                ILGenerator methodIl = set.GetILGenerator();

                methodIl.Emit(OpCodes.Ldarg_0);
                methodIl.Emit(OpCodes.Ldarg_1);
                methodIl.Emit(OpCodes.Stfld, field);
                methodIl.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(set);
            }
        }
    }
}