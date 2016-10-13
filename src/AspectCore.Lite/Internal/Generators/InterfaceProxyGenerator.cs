using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.Lite.Generators
{
    public sealed class InterfaceProxyGenerator: ProxyGenerator
    {
        public InterfaceProxyGenerator(IServiceProvider serviceProvider, Type serviceType, params Type[] impInterfaceTypes) :
            base(serviceProvider, serviceType, impInterfaceTypes)
        {

            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type {serviceType} should be interface.", nameof(serviceType));
            }
        }

        public override Type GenerateProxyType()
        {
            return emitBuilderProvider.DefinedType(serviceType, key =>
            {
                return GenerateProxyTypeInfo(key).AsType();
            });
        }

        protected override TypeBuilder GenerateTypeBuilder()
        {
            return emitBuilderProvider.CurrentModuleBuilder.DefineType($"{serviceType.Namespace}.{GeneratorConstants.Interface}{serviceType.Name}", TypeAttributes.Class | TypeAttributes.Public, typeof(object), impInterfaceTypes);
        }

        private TypeInfo GenerateProxyTypeInfo(Type key)
        {
            var interfaceTypes = new Type[] { key }.Concat(impInterfaceTypes).ToArray();    
            var serviceProviderGenerator = new FieldGenerator(TypeBuilder, typeof(IServiceProvider), GeneratorConstants.ServiceProvider);
            var serviceInstanceGenerator = new FieldGenerator(TypeBuilder, key, GeneratorConstants.ServiceInstance);

            var constructorGenerator = new InterfaceConstructorGenerator(TypeBuilder, key, serviceProviderGenerator, serviceInstanceGenerator);

            constructorGenerator.GenerateConstructor();

            foreach(var impType in interfaceTypes)
            {
                GenerateInterfaceProxy(key, serviceProviderGenerator, serviceInstanceGenerator);
            }

            return TypeBuilder.CreateTypeInfo();
        }
    }
}
