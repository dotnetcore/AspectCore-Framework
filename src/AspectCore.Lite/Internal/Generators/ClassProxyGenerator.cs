using AspectCore.Lite.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public sealed class ClassProxyGenerator : ProxyGenerator
    {
        public ClassProxyGenerator(IServiceProvider serviceProvider, Type parentType, params Type[] impInterfaceTypes)
            : base(serviceProvider, parentType, impInterfaceTypes)
        {
            if (!parentType.GetTypeInfo().IsClass)
            {
                throw new ArgumentException($"Type {parentType} should be class.", nameof(parentType));
            }

            if (parentType.GetTypeInfo().IsSealed)
            {
                throw new ArgumentException($"Type {parentType} cannot be sealed.", nameof(parentType));
            }
        }

        private TypeInfo GenerateProxyTypeInfo(Type key)
        {

            var serviceProviderGenerator = new FieldGenerator(TypeBuilder, typeof(IServiceProvider), GeneratorConstants.ServiceProvider);
            var serviceInstanceGenerator = new FieldGenerator(TypeBuilder, key, GeneratorConstants.ServiceInstance);

            GenerateClassProxy(serviceType, serviceProviderGenerator, serviceInstanceGenerator);

            foreach (var impType in impInterfaceTypes)
            {
                GenerateInterfaceProxy(impType, serviceProviderGenerator, serviceInstanceGenerator);
            }

            return TypeBuilder.CreateTypeInfo();
        }

        protected override TypeBuilder GenerateTypeBuilder()
        {
            return emitBuilderProvider.CurrentModuleBuilder.DefineType($"{serviceType.Namespace}.{GeneratorConstants.Class}{serviceType.Name}", TypeAttributes.Class | TypeAttributes.Public, serviceType, Type.EmptyTypes);
        }

        public override Type GenerateProxyType()
        {
            return emitBuilderProvider.DefinedType(serviceType, key =>
            {
                return GenerateProxyTypeInfo(key).AsType();
            });
        }
    }
}
