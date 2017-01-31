using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal sealed class NonProxyMethodBodyGenerator : MethodBodyGenerator
    {
        private readonly MethodInfo parentMethod;
        private readonly FieldBuilder serviceInstanceFieldBuilder;

        public NonProxyMethodBodyGenerator(MethodBuilder declaringMember, MethodInfo parentMethod, FieldBuilder serviceInstanceField)
            : base(declaringMember)
        {
            this.parentMethod = parentMethod;
            this.serviceInstanceFieldBuilder = serviceInstanceField;
        }

        protected override void GeneratingMethodBody(ILGenerator ilGenerator)
        {
            var parameters = parentMethod.GetParameterTypes();
            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, serviceInstanceFieldBuilder);
            for (int i = 1; i <= parameters.Length; i++)
            {
                ilGenerator.EmitLoadArg(i);
            }
            ilGenerator.Emit(parentMethod.IsCallByLookupVTable() ? OpCodes.Callvirt : OpCodes.Call, parentMethod);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
