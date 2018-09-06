using System;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    /// <summary>
    /// Private structure to store information about a member of a new type.
    /// </summary>
    readonly struct MemberDefinition
    {
        /// <summary>
        /// Defines the method attributes needed to generate CLR compliant getter and setters in IL code.
        /// </summary>
        [NotNull] const MethodAttributes GetSetAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;

        /// <summary>
        /// The name of the member.
        /// </summary>
        [NotNull] readonly string _name;

        /// <summary>
        /// The underlying type of the member being defined.
        /// </summary>
        [NotNull] readonly Type _type;

        /// <summary>
        /// The private backing field for the public property.
        /// </summary>
        [NotNull] readonly FieldBuilder _fieldBuilder;

        /// <summary>
        /// Defines the public property backed by the private field.
        /// </summary>
        [NotNull] readonly PropertyBuilder _propertyBuilder;

        /// <summary>
        /// Constructs an instance of the <see cref="MemberDefinition"/> struct.
        /// </summary>
        /// <param name="propertyInfo">The name and type of the property.</param>
        /// <param name="typeBuilder">The type builder to mutate.</param>
        internal MemberDefinition((string Name, Type Type) propertyInfo, [NotNull] TypeBuilder typeBuilder)
        {
            _name = propertyInfo.Name;
            _type = propertyInfo.Type;
            _fieldBuilder = typeBuilder.DefineField($"_{_name}", _type, FieldAttributes.Private);
            _propertyBuilder = typeBuilder.DefineProperty(_name, PropertyAttributes.None, _type, Type.EmptyTypes);

            DefineGetter(typeBuilder);
            DefineSetter(typeBuilder);
        }

        /// <summary>
        /// Implicitly casts from <see cref="T:ApiLibrary.Expressions.MemberDefinition"/> to <see cref="T:System.Reflection.Type"/>.
        /// </summary>
        [NotNull]
        public static explicit operator Type(MemberDefinition memberDefinition) => memberDefinition._type;

        /// <summary>
        /// Implicitly casts from <see cref="T:AD.ApiExtensions.Expressions.MemberDefinition"/> to <see cref="T:System.Reflection.FieldInfo"/>.
        /// </summary>
        [NotNull]
        public static explicit operator FieldInfo(MemberDefinition memberDefinition) => memberDefinition._fieldBuilder;

        /// <summary>
        /// Defines the method body the GetMethod of this member definition.
        /// </summary>
        void DefineGetter([NotNull] TypeBuilder typeBuilder)
        {
            MethodBuilder getMethod = typeBuilder.DefineMethod($"get_{_name}", GetSetAttributes, _type, Type.EmptyTypes);
            ILGenerator methodIl = getMethod.GetILGenerator();

            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldfld, _fieldBuilder);
            methodIl.Emit(OpCodes.Ret);

            _propertyBuilder.SetGetMethod(getMethod);
        }

        /// <summary>
        /// Defines the method body the SetMethod of this member definition.
        /// </summary>
        void DefineSetter([NotNull] TypeBuilder typeBuilder)
        {
            MethodBuilder setMethod = typeBuilder.DefineMethod($"set_{_name}", GetSetAttributes, null, new Type[] { _type });
            ILGenerator methodIl = setMethod.GetILGenerator();

            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldarg_1);
            methodIl.Emit(OpCodes.Stfld, _fieldBuilder);
            methodIl.Emit(OpCodes.Ret);

            _propertyBuilder.SetSetMethod(setMethod);
        }
    }
}