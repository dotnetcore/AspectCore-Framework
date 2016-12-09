using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class FieldGenerator : Generator<TypeBuilder, FieldBuilder>
    {
        public FieldGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        public abstract string FieldName { get; }

        public abstract Type FieldType { get; }

        public abstract FieldAttributes FieldAttributes { get; }

        protected internal override FieldBuilder Accept(GeneratorVisitor visitor)
        {
            return DeclaringMember.DefineField(FieldName, FieldType, FieldAttributes);
        }
    }
}
