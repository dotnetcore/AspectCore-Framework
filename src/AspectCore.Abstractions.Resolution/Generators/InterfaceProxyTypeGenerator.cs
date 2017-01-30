using AspectCore.Abstractions.Extensions;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class InterfaceProxyTypeGenerator : ProxyTypeGenerator
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

        public override string TypeName => $"{ModuleGenerator.ProxyNameSpace}.ServiceInstance.Proxy{ServiceType.Name}";

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
                    new NonProxyMethodGenerator(declaringType, method, serviceInstanceFieldBuilder, false).Build();
                    continue;
                }
                new ProxyMethodGenerator(declaringType, ServiceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, false).Build();
            }
        }

        protected override void GeneratingProperty(TypeBuilder declaringType)
        {
            foreach (var property in ServiceType.GetTypeInfo().DeclaredProperties)
            {
                if (AspectValidator.Validate(property))
                {
                    new ProxyPropertyGenerator(declaringType, property, ServiceType, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, false).Build();
                }
                else
                {
                    new NonProxyPropertyGenerator(declaringType, property, ServiceType, serviceInstanceFieldBuilder, false).Build();
                }
            }
        }
    }
}