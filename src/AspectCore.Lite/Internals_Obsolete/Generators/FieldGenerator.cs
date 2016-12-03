using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internals.Generators
{
    internal sealed class FieldGenerator
    {
        internal FieldGenerator(TypeBuilder typeBuilder, Type fieldType, string fieldName)
        {
            FieldType = fieldType;
            FieldBuilder = typeBuilder.DefineField($"{GeneratorConstants.Field}{fieldName}", fieldType, FieldAttributes.Private);
        }

        public FieldBuilder FieldBuilder { get; }

        public  Type FieldType { get; }
    }
}
