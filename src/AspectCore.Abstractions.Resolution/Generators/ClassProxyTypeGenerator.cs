using AspectCore.Abstractions.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class ClassProxyTypeGenerator : ProxyTypeGenerator
    {
        public ClassProxyTypeGenerator(Type serviceType, IAspectValidator aspectValidator) : base(serviceType, aspectValidator)
        {
            if (!serviceType.GetTypeInfo().IsClass)
            {
                throw new ArgumentException($"Type '{serviceType}' must be class.", nameof(serviceType));
            }
        }

        public override Type[] Interfaces => Type.EmptyTypes;

        public override Type ParentType => ServiceType;

        protected override void GeneratingConstructor(TypeBuilder declaringType)
        {
            if (!ServiceType.GetTypeInfo().IsAbstract)
            {
                new DefaultProxyConstructorGenerator(declaringType, ServiceType, MethodInfoConstant.Object_Ctor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
                return;
            }
            var constructors = ServiceType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyAndAssembly || c.IsFamilyOrAssembly)).ToArray();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"A suitable constructor for type {ServiceType.FullName} could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
            }
            foreach (var constructor in constructors)
            {
                new ProxyConstructorGenerator(declaringType, ServiceType, constructor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
            }
        }

        protected override void GeneratingMethod(TypeBuilder declaringType)
        {
            foreach (var method in ServiceType.GetTypeInfo().DeclaredMethods)
            {
                if (method.IsPropertyBinding())
                {
                    continue;
                }
                if (AspectValidator.Validate(method))
                {
                    new ProxyMethodGenerator(declaringType, ServiceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
                    continue;
                }
                if (method.IsVirtual)
                {
                    new NonProxyMethodGenerator(declaringType, method, serviceInstanceFieldBuilder).Build();
                }
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
                else if (property.IsVirtual())
                {
                    new NonProxyPropertyGenerator(declaringType, property, ServiceType, serviceInstanceFieldBuilder).Build();
                }
            }
        }
    }
}