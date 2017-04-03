using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Generator
{
    public abstract class FieldGenerator : AbstractGenerator<TypeBuilder, FieldBuilder>
    {
        protected FieldGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        public abstract string FieldName { get; }

        public abstract Type FieldType { get; }

        public abstract FieldAttributes FieldAttributes { get; }

        protected override FieldBuilder ExecuteBuild()
        {
            return DeclaringMember.DefineField(FieldName, FieldType, FieldAttributes);
        }
    }
}
