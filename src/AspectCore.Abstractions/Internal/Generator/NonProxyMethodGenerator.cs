using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;

namespace AspectCore.Abstractions.Internal.Generator
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
            var parentMethod = _parentType.GetTypeInfo().GetMethodBySign(_serviceMethod);
            return new NonProxyMethodBodyGenerator(declaringMethod, parentMethod ?? _serviceMethod, _serviceInstanceFieldBuilder);
        }
    }
}