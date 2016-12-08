using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class GenericMethodGenerator : MethodGenerator
    {
        public abstract bool IsGenericMethod { get; }

        public GenericMethodGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override MethodBuilder Accept(GeneratorVisitor visitor)
        {
            var methodBuilder = base.Accept(visitor);
            if (IsGenericMethod)
            {
                GeneratingGenericParameter(methodBuilder);
            }
            return methodBuilder;
        }

        protected abstract void GeneratingGenericParameter(MethodBuilder declaringMethod);
    }
}
