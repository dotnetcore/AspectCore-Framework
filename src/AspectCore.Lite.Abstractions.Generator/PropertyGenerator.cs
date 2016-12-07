using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class PropertyGenerator : Generator<TypeBuilder, PropertyBuilder>
    {
        public abstract string PropertyName { get; }

        public abstract PropertyAttributes PropertyAttributes { get; }

        public abstract CallingConventions CallingConventions { get; }

        public abstract Type ReturnType { get; }

        public abstract bool CanRead { get; }

        public abstract bool CanWrite { get; }

        public virtual Type[] ParameterTypes
        {
            get
            {
                return Type.EmptyTypes;
            }
        }

        public PropertyGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override PropertyBuilder Accept(GeneratorVisitor visitor)
        {
            var propertyBuilder = DeclaringMember.DefineProperty(PropertyName, PropertyAttributes, CallingConventions, ReturnType, ParameterTypes);

            if (CanRead)
            {
                var readMethodGenerator = GetReadMethodGenerator(DeclaringMember);
                var methodBuilder = (MethodBuilder)visitor.VisitGenerator(readMethodGenerator);
                propertyBuilder.SetGetMethod(methodBuilder);
            }

            if (CanWrite)
            {
                var writeMethodGenerator = GetWriteMethodGenerator(DeclaringMember);
                var methodBuilder = (MethodBuilder)visitor.VisitGenerator(writeMethodGenerator);
                propertyBuilder.SetSetMethod(methodBuilder);
            }

            return propertyBuilder;
        }

        protected abstract MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType);

        protected abstract MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType);
    }
}
