using AspectCore.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal sealed class NonProxyMethodGenerator : ProxyMethodGenerator
    {
        const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        public NonProxyMethodGenerator(TypeBuilder declaringMember, MethodInfo serviceMethod, FieldBuilder serviceInstanceFieldBuilder)
            : base(declaringMember, serviceMethod.DeclaringType, null, serviceMethod, serviceInstanceFieldBuilder, null)
        {
        }

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            return new NonProxyMethodBodyGenerator(declaringMethod, serviceMethod, serviceInstanceFieldBuilder);
        }
    }
}
