using AspectCore.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class NonProxyPropertyGenerator : ProxyPropertyGenerator
    {
        public NonProxyPropertyGenerator(TypeBuilder declaringMember, PropertyInfo propertyInfo, Type serviceType, FieldBuilder serviceInstanceFieldBuilder)
            : base(declaringMember, propertyInfo, serviceType, null, serviceInstanceFieldBuilder, null)
        {
        }

        protected override MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType)
        {
            return new NonProxyMethodGenerator(declaringType, GetMethod, serviceInstanceFieldBuilder);
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            return new NonProxyMethodGenerator(declaringType, SetMethod, serviceInstanceFieldBuilder);
        }
    }
}