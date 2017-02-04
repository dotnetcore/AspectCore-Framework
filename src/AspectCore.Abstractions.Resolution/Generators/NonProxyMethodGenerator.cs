using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal sealed class NonProxyMethodGenerator : ProxyMethodGenerator
    {
        const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        public NonProxyMethodGenerator(TypeBuilder declaringMember, Type parentType, MethodInfo serviceMethod, FieldBuilder serviceInstanceFieldBuilder, bool isImplementExplicitly)
            : base(declaringMember, serviceMethod.DeclaringType, parentType, serviceMethod, serviceInstanceFieldBuilder, null, isImplementExplicitly)
        {
        }

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            var parentMethod = parentType.GetTypeInfo().GetMethodBySign(serviceMethod);
            return new NonProxyMethodBodyGenerator(declaringMethod, parentMethod ?? serviceMethod, serviceInstanceFieldBuilder);
        }
    }
}