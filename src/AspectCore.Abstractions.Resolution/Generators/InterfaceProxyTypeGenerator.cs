using AspectCore.Abstractions.Extensions;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal sealed class InterfaceProxyTypeGenerator : ProxyTypeGenerator
    {
        public InterfaceProxyTypeGenerator(Type serviceType, IAspectValidator aspectValidator) : base(serviceType, aspectValidator)
        {
            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' must be interface.", nameof(serviceType));
            }
        }

        public override Type[] Interfaces => new Type[] { ServiceType };

        public override Type ParentType => typeof(object);

        protected override void GeneratingConstructor(TypeBuilder declaringType)
        {
            new DefaultProxyConstructorGenerator(declaringType, ServiceType, MethodInfoConstant.Object_Ctor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
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
                    new NonProxyMethodGenerator(declaringType, method, serviceInstanceFieldBuilder).Build();
                    continue;
                }
                new ProxyMethodGenerator(declaringType, ServiceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
            }
        }

        protected override void GeneratingProperty(TypeBuilder declaringType)
        {
            foreach (var property in ServiceType.GetTypeInfo().DeclaredProperties)
            {
                if (AspectValidator.Validate(property))
                {
                    new ProxyPropertyGenerator(declaringType, property, ServiceType, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
                }
                else
                {
                    new NonProxyPropertyGenerator(declaringType, property, ServiceType, serviceInstanceFieldBuilder).Build();
                }
            }
        }
    }
}