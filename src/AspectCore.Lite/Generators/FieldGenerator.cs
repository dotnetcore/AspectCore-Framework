using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public sealed class FieldGenerator
    {
        private readonly FieldBuilder builder;
        internal FieldGenerator(TypeBuilder typeBuilder, Type fieldType, string fieldName)
        {
            builder = typeBuilder.DefineField($"{GeneratorConstants.Field}{fieldName}", fieldType, FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        public FieldBuilder FieldBuilder => builder;
    }
}
