using AspectCore.Abstractions.Extensions;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class InheritanceInterfaceProxyTypeGenerator : ClassProxyTypeGenerator
    {
        public InheritanceInterfaceProxyTypeGenerator(Type serviceType, Type parentType, IAspectValidator aspectValidator)
            : base(serviceType, parentType, null, aspectValidator)
        {
        }

        protected override void GeneratingMethod(TypeBuilder declaringType)
        {
            foreach (var method in ServiceType.GetTypeInfo().DeclaredMethods)
            {
                if (method.IsPropertyBinding())
                {
                    continue;
                }
                if (!AspectValidator.Validate(method))
                {
                    new NonProxyMethodGenerator(declaringType, method, serviceInstanceFieldBuilder, true).Build();
                    continue;
                }
                new ProxyMethodGenerator(declaringType, ServiceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, true).Build();
            }
        }

        protected override void GeneratingProperty(TypeBuilder declaringType)
        {
            foreach (var property in ServiceType.GetTypeInfo().DeclaredProperties)
            {
                if (AspectValidator.Validate(property))
                {
                    new ProxyPropertyGenerator(declaringType, property, ServiceType, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, true).Build();
                }
                else
                {
                    new NonProxyPropertyGenerator(declaringType, property, ServiceType, serviceInstanceFieldBuilder, true).Build();
                }
            }
        }
    }
}
