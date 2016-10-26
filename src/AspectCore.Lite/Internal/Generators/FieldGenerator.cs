using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internal.Generators
{
    internal sealed class FieldGenerator
    {
        private readonly FieldBuilder builder;
        internal FieldGenerator(TypeBuilder typeBuilder, Type fieldType, string fieldName)
        {
            builder = typeBuilder.DefineField($"{GeneratorConstants.Field}{fieldName}", fieldType, FieldAttributes.Private);
        }

        public FieldBuilder FieldBuilder => builder;
    }
}
