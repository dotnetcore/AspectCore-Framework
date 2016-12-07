using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class MethodGenerator : Generator<TypeBuilder, MethodBuilder>
    {
        public abstract string MethodName { get; }

        public abstract MethodAttributes MethodAttributes { get; }

        public abstract CallingConventions CallingConventions { get; }

        public abstract Type ReturnType { get; }

        public abstract Type[] ParameterTypes { get; }

        public MethodGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override MethodBuilder Accept(GeneratorVisitor visitor)
        {
            var methodBuilder = DeclaringMember.DefineMethod(MethodName, MethodAttributes, CallingConventions, ReturnType, ParameterTypes);

            var methodBodyGenerator = GetMethodBodyGenerator(methodBuilder);
            if (methodBodyGenerator != null)
            {
                visitor.VisitGenerator(methodBodyGenerator);
            }

            return methodBuilder;
        }

        protected abstract MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod);
    }
}
