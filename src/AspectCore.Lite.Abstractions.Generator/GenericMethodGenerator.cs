using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class GenericMethodGenerator : MethodGenerator
    {
        public abstract bool IsGenericMethod { get; }

        public GenericMethodGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected internal override MethodBuilder Accept(GeneratorVisitor visitor)
        {
            var methodBuilder = DeclaringMember.DefineMethod(MethodName, MethodAttributes, CallingConventions, ReturnType, ParameterTypes);
            if (IsGenericMethod)
            {
                GeneratingGenericParameter(methodBuilder);
            }
            var methodBodyGenerator = GetMethodBodyGenerator(methodBuilder);
            if (methodBodyGenerator != null)
            {
                visitor.VisitGenerator(methodBodyGenerator);
            }
            return methodBuilder;
        }

        protected internal abstract void GeneratingGenericParameter(MethodBuilder declaringMethod);
    }
}
