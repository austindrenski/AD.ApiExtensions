using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Types
{
    /// <summary>
    /// Defines a new anonymous type.
    /// </summary>
    [PublicAPI]
    public readonly struct TypeDefinition : IEquatable<TypeDefinition>
    {
        /// <summary>
        /// The name of the dynamic assembly and dynamic module.
        /// </summary>
        [NotNull] const string AnonymousAssemblyName = "AD.ApiExtensions.AnonymousTypes";

        /// <summary>
        /// The dynamic module builder used to define new types.
        /// </summary>
        [NotNull] static readonly ModuleBuilder ModuleBuilder =
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AnonymousAssemblyName), AssemblyBuilderAccess.Run)
                           .DefineDynamicModule(AnonymousAssemblyName);

        /// <summary>
        /// The static method info for <see cref="T:object.Equals(object,object)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo ObjectEquals =
            typeof(object).GetRuntimeMethod(nameof(object.Equals), new Type[] { typeof(object), typeof(object) });

        /// <summary>
        /// The static method info for <see cref="T:object.GetHashCode()"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo ObjectGetHashCode =
            typeof(object).GetRuntimeMethod(nameof(object.GetHashCode), Type.EmptyTypes);

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
                    $"@__anonymous_{properties.Length}_{Interlocked.Increment(ref _typeCount)}",
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
        /// The type is named with the pattern: @__anonymous__{properties}_{id}.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="properties"/></exception>
        [NotNull]
        public static Type GetOrAdd([NotNull] (string Name, Type Type)[] properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            int hashCode = 397;

            for (int i = 0; i < properties.Length; i++)
            {
                unchecked
                {
                    hashCode ^= 397 * properties[i].GetHashCode();
                }
            }

            return Types.GetOrAdd(hashCode, _ => new TypeDefinition(properties));
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
            Type[] fieldTypes = new Type[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = new MemberDefinition(builder, properties[i].Name, properties[i].Type);
                fieldTypes[i] = fields[i].FieldType;
            }

            const CallingConventions ctorConventions =
                CallingConventions.Standard | CallingConventions.HasThis;

            const MethodAttributes ctorAttributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

            ConstructorBuilder ctor =
                builder.DefineConstructor(ctorAttributes, ctorConventions, fieldTypes);

            ILGenerator il = ctor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, builder.DefineDefaultConstructor(ctorAttributes));

            for (int i = 0; i < fields.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Stfld, fields[i]);
            }

            il.Emit(OpCodes.Ret);

            DefineGetHashCode(builder, fields);
            DefineTypedEquals(builder, fields);
        }

        static void DefineGetHashCode([NotNull] TypeBuilder builder, [NotNull] [ItemNotNull] FieldInfo[] fields)
        {
            const MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            MethodBuilder getHashCode =
                builder.DefineMethod(nameof(ValueType.GetHashCode), methodAttributes, typeof(int), Type.EmptyTypes);

            ILGenerator il = getHashCode.GetILGenerator();

            il.Emit(OpCodes.Ldc_I4, 397);

            for (int i = 0; i < fields.Length; i++)
            {
                Label end = il.DefineLabel();
                Label isNull = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldflda, fields[i]);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Brfalse_S, isNull);

                // N.B. callvirt normally takes a reference, but constrained -> callvirt requires an address.
                il.Emit(OpCodes.Constrained, fields[i].FieldType);
                il.Emit(OpCodes.Callvirt, ObjectGetHashCode);
                il.Emit(OpCodes.Br_S, end);

                il.MarkLabel(isNull);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ldc_I4_0);

                il.MarkLabel(end);
                il.Emit(OpCodes.Ldc_I4, 397);
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Xor);
            }

            il.Emit(OpCodes.Ret);
        }

        static void DefineTypedEquals([NotNull] TypeBuilder builder, [NotNull] [ItemNotNull] FieldInfo[] fields)
        {
            const MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            MethodBuilder typedEquals =
                builder.DefineMethod(nameof(ValueType.Equals), methodAttributes, typeof(bool), new Type[] { builder });

            ILGenerator il = typedEquals.GetILGenerator();

            il.Emit(OpCodes.Ldc_I4_1);

            for (int i = 0; i < fields.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fields[i]);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldfld, fields[i]);
                il.Emit(OpCodes.Call, ObjectEquals);
                il.Emit(OpCodes.Ceq);
            }

            il.Emit(OpCodes.Ret);

            DefineObjectEquals(builder, typedEquals);
            DefineOpEquality(builder, typedEquals);
            DefineOpInequality(builder, typedEquals);
        }

        static void DefineObjectEquals([NotNull] TypeBuilder builder, [NotNull] MethodInfo typedEquals)
        {
            const MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            MethodBuilder objectEquals =
                builder.DefineMethod(nameof(ValueType.Equals), methodAttributes, typeof(bool), new Type[] { typeof(object) });

            ILGenerator il = objectEquals.GetILGenerator();
            Label isFalse = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Isinst, builder);
            il.Emit(OpCodes.Brfalse_S, isFalse);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, builder);
            il.Emit(OpCodes.Call, typedEquals);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(isFalse);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
        }

        static void DefineOpEquality([NotNull] TypeBuilder builder, [NotNull] MethodInfo typedEquals)
        {
            const MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.SpecialName;

            MethodBuilder opEquality =
                builder.DefineMethod("op_Equality", methodAttributes, typeof(bool), new Type[] { builder, builder });

            ILGenerator il = opEquality.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typedEquals);
            il.Emit(OpCodes.Ret);
        }

        static void DefineOpInequality([NotNull] TypeBuilder builder, [NotNull] MethodInfo typedEquals)
        {
            const MethodAttributes methodAttributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.SpecialName;

            MethodBuilder opInequality =
                builder.DefineMethod("op_Inequality", methodAttributes, typeof(bool), new Type[] { builder, builder });

            ILGenerator il = opInequality.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typedEquals);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Private structure to define a public property and a backing field.
        /// </summary>
        private readonly struct MemberDefinition
        {
            /// <summary>
            /// The backing field for the public property.
            /// </summary>
            [NotNull] readonly FieldInfo _fieldInfo;

            /// <summary>
            /// Constructs an instance of the <see cref="MemberDefinition"/> struct.
            /// </summary>
            /// <param name="typeBuilder">The type builder to mutate.</param>
            /// <param name="propertyName">The name of the property.</param>
            /// <param name="propertyType">The type of the property.</param>
            internal MemberDefinition([NotNull] TypeBuilder typeBuilder, [NotNull] string propertyName, [NotNull] Type propertyType)
            {
                _fieldInfo =
                    typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

                PropertyBuilder propertyBuilder =
                    typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, _fieldInfo.FieldType, Type.EmptyTypes);

                propertyBuilder.SetGetMethod(DefineGetter(typeBuilder, _fieldInfo));
                propertyBuilder.SetSetMethod(DefineSetter(typeBuilder, _fieldInfo));
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
            [NotNull]
            static MethodBuilder DefineGetter([NotNull] TypeBuilder builder, [NotNull] FieldInfo field)
            {
                const MethodAttributes propertyAttributes =
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;

                MethodBuilder getter =
                    builder.DefineMethod($"get{field.Name}", propertyAttributes, field.FieldType, Type.EmptyTypes);

                ILGenerator il = getter.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ret);

                return getter;
            }

            /// <summary>
            /// Defines the method body the SetMethod of this member definition.
            /// </summary>
            [NotNull]
            static MethodBuilder DefineSetter([NotNull] TypeBuilder builder, [NotNull] FieldInfo field)
            {
                const MethodAttributes propertyAttributes =
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;

                MethodBuilder setter =
                    builder.DefineMethod($"set{field.Name}", propertyAttributes, typeof(void), new Type[] { field.FieldType });

                ILGenerator il = setter.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Ret);

                return setter;
            }
        }
    }
}