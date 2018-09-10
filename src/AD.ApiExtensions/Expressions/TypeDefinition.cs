using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    /// <summary>
    /// Private structure to construct a new type.
    /// </summary>
    readonly struct TypeDefinition
    {
        [NotNull] private const string AnonymousAssemblyName = "AD.ApiExtensions.Anonymous";

        [NotNull] private const TypeAttributes AnonymousTypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.Serializable;

        [NotNull] private static readonly ModuleBuilder ModuleBuilder;

        [NotNull] private static readonly ConstructorInfo BaseConstructorInfo;

        [NotNull] private readonly TypeBuilder _typeBuilder;

        private static long _typeCounter;

        static TypeDefinition()
        {
            ModuleBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AnonymousAssemblyName), AssemblyBuilderAccess.Run).DefineDynamicModule(AnonymousAssemblyName);
            BaseConstructorInfo = typeof(object).GetEmptyConstructor();
        }

        internal TypeDefinition([NotNull] IEnumerable<PropertyInfo> properties)
            : this(properties.Select(x => (x.Name, x.PropertyType)))
        {
        }

        internal TypeDefinition([NotNull] IEnumerable<(string Name, Type Type)> properties)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            long next = Interlocked.Increment(ref _typeCounter);

            _typeBuilder = ModuleBuilder.DefineType($"f__Anonymous__{next}", AnonymousTypeAttributes, typeof(ValueType));
            DefineConstructor();
            DefineConstructorForConstructorInjection(properties);
        }

        /// <summary>
        /// Implicitly casts from <see cref="T:AD.ApiExtensions.Expressions.TypeDefinition"/> to <see cref="T:System.Reflection.TypeInfo"/>.
        /// </summary>
        public static explicit operator TypeInfo(TypeDefinition typeDefinition)
        {
            return typeDefinition._typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Defines a default parameterless constructor.
        /// </summary>
        private void DefineConstructor()
        {
            _typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
        }

        /// <summary>
        /// Defines a constructor that assigns values to the specified members during construction.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyInfo"/> describing the properties to be injected during construction.</param>
        private void DefineConstructorForConstructorInjection([NotNull] IEnumerable<(string Name, Type Type)> properties)
        {
            (string Name, Type Type)[] propertyInfo = properties as (string Name, Type Type)[] ?? properties.ToArray();
            MemberDefinition[] memberDefinitions = new MemberDefinition[propertyInfo.Length];
            Type[] memberTypes = new Type[propertyInfo.Length];

            for (int i = 0; i < memberDefinitions.Length; i++)
            {
                memberDefinitions[i] = new MemberDefinition(propertyInfo[i], _typeBuilder);
                memberTypes[i] = (Type) memberDefinitions[i];
            }

            ILGenerator constructorIl =
                _typeBuilder.DefineConstructor(
                                MethodAttributes.PrivateScope,
                                CallingConventions.Standard,
                                memberTypes)
                            .GetILGenerator();

            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Call, BaseConstructorInfo);

            for (int i = 0; i < memberDefinitions.Length; i++)
            {
                constructorIl.Emit(OpCodes.Ldarg_0);
                constructorIl.Emit(OpCodes.Ldarg, i + 1);
                constructorIl.Emit(OpCodes.Stfld, (FieldBuilder) memberDefinitions[i]);
            }

            constructorIl.Emit(OpCodes.Ret);
        }
    }
}