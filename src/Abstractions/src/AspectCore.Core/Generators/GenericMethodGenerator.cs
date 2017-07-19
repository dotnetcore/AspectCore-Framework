using System;
using System.Reflection.Emit;
using AspectCore.Abstractions;

namespace AspectCore.Core.Generator
{
    [NonAspect]
    public abstract class GenericMethodGenerator : MethodGenerator
    {
        public abstract bool IsGenericMethod { get; }

        protected GenericMethodGenerator(TypeBuilder declaringMember) : base(declaringMember)
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
