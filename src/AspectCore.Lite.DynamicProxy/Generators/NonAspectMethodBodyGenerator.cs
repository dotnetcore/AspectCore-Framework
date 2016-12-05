using AspectCore.Lite.Abstractions.Generator;
using AspectCore.Lite.DynamicProxy.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal sealed class NonAspectMethodBodyGenerator : MethodBodyGenerator
    {
        private readonly MethodInfo targetMethod;
        private readonly FieldBuilder serviceInstanceFieldBuilder;

        public NonAspectMethodBodyGenerator(MethodBuilder declaringMember, MethodInfo targetMethod, FieldBuilder serviceInstanceField)
            : base(declaringMember)
        {
            this.targetMethod = targetMethod;
            this.serviceInstanceFieldBuilder = serviceInstanceField;
        }

        protected override void GeneratingMethodBody(ILGenerator ilGenerator)
        {
            var parameters = targetMethod.GetParameters().Select(x => x.ParameterType).ToArray();
            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, serviceInstanceFieldBuilder);
            for (int i = 1; i <= parameters.Length; i++)
            {
                ilGenerator.EmitLoadArg(i);
            }
            ilGenerator.Emit(OpCodes.Callvirt, targetMethod);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
