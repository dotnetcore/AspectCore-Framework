using AspectCore.Abstractions.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class InheritanceInterfaceProxyTypeGenerator : ClassProxyTypeGenerator
    {
        public InheritanceInterfaceProxyTypeGenerator(Type serviceType, Type parentType, Type[] interfaces, IAspectValidator aspectValidator)
            : base(serviceType, parentType, interfaces, aspectValidator)
        {
            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' should be interface.", nameof(serviceType));
            }
            Interfaces = new Type[] { serviceType }.Concat(interfaces ?? Type.EmptyTypes).ToArray();
        }

        public override Type[] Interfaces { get; }

        protected override void GeneratingMethod(TypeBuilder declaringType)
        {
            foreach (var interfaceType in Interfaces)
            {
                foreach (var method in interfaceType.GetTypeInfo().DeclaredMethods)
                {
                    if (method.IsPropertyBinding())
                    {
                        continue;
                    }
                    if (!AspectValidator.Validate(method))
                    {
                        new NonProxyMethodGenerator(declaringType, ParentType, method, serviceInstanceFieldBuilder, true).Build();
                        continue;
                    }
                    new ProxyMethodGenerator(declaringType, interfaceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, true).Build();
                }
            }
        }

        protected override void GeneratingProperty(TypeBuilder declaringType)
        {
            foreach (var interfaceType in Interfaces)
            {
                foreach (var property in interfaceType.GetTypeInfo().DeclaredProperties)
                {
                    if (AspectValidator.Validate(property))
                    {
                        new ProxyPropertyGenerator(declaringType, property, interfaceType, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, true).Build();
                    }
                    else
                    {
                        new NonProxyPropertyGenerator(declaringType, property, interfaceType, ParentType, serviceInstanceFieldBuilder, true).Build();
                    }
                }
            }
        }
    }
}
