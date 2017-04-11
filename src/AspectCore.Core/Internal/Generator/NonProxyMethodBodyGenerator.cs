using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Internal;
using AspectCore.Abstractions.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal sealed class NonProxyMethodBodyGenerator : MethodBodyGenerator
    {
        private readonly MethodInfo _parentMethod;
        private readonly FieldBuilder _serviceInstanceFieldBuilder;

        public NonProxyMethodBodyGenerator(MethodBuilder declaringMember, MethodInfo parentMethod, FieldBuilder serviceInstanceField)
            : base(declaringMember)
        {
            this._parentMethod = parentMethod;
            this._serviceInstanceFieldBuilder = serviceInstanceField;
        }

        protected override void GeneratingMethodBody(ILGenerator ilGenerator)
        {
            var parameters = _parentMethod.GetParameterTypes();
            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, _serviceInstanceFieldBuilder);
            for (int i = 1; i <= parameters.Length; i++)
            {
                ilGenerator.EmitLoadArg(i);
            }
            ilGenerator.Emit(_parentMethod.IsCallvirt() ? OpCodes.Callvirt : OpCodes.Call, _parentMethod);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
