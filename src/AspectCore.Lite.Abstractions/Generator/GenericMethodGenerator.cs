using System;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class GenericMethodGenerator : MethodGenerator
    {
        public abstract bool IsGenericMethod { get; }

        public GenericMethodGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override MethodBuilder ExecuteBuild()
        {
            var methodBuilder = DeclaringMember.DefineMethod(MethodName, MethodAttributes, CallingConventions, ReturnType, ParameterTypes);
            if (IsGenericMethod)
            {
                GeneratingGenericParameter(methodBuilder);
            }
            GetMethodBodyGenerator(methodBuilder)?.Build();
            return methodBuilder;
        }

        protected internal abstract void GeneratingGenericParameter(MethodBuilder declaringMethod);
    }
}
