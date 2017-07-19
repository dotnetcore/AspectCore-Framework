using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class NonProxyPropertyGenerator : ProxyPropertyGenerator
    {
        public NonProxyPropertyGenerator(TypeBuilder declaringMember, PropertyInfo propertyInfo, Type serviceType, Type parentType, FieldBuilder serviceInstanceFieldBuilder, bool isImplementExplicitly)
            : base(declaringMember, propertyInfo, serviceType, parentType, serviceInstanceFieldBuilder, null, isImplementExplicitly)
        {
        }

        protected override MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType)
        {
            return new NonProxyMethodGenerator(declaringType, _parentType, GetMethod, _serviceInstanceFieldBuilder, _isImplementExplicitly);
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            return new NonProxyMethodGenerator(declaringType, _parentType, SetMethod, _serviceInstanceFieldBuilder, _isImplementExplicitly);
        }
    }
}