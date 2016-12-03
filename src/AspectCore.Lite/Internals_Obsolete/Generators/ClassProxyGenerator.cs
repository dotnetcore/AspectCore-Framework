using AspectCore.Lite.Common;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internals.Generators
{
    internal sealed class ClassProxyGenerator : ProxyGenerator
    {
        private readonly Type serviceInstanceType;
        public ClassProxyGenerator(IServiceProvider serviceProvider ,Type serviceInstanceType, Type serviceType, params Type[] impInterfaceTypes)
            : base(serviceProvider , serviceType, impInterfaceTypes)
        {
            ExceptionHelper.ThrowArgument(() => !serviceType.GetTypeInfo().IsClass , $"Type {serviceType} should be class." , nameof(serviceType));
            ExceptionHelper.ThrowArgument(() => serviceType.GetTypeInfo().IsSealed , $"Type {serviceType} cannot be sealed." , nameof(serviceType));
            this.serviceInstanceType = serviceInstanceType;
        }

        private TypeInfo GenerateProxyTypeInfo(Type key)
        {

            var serviceProviderGenerator = new FieldGenerator(TypeBuilder, typeof(IServiceProvider), GeneratorConstants.ServiceProvider);
            var serviceInstanceGenerator = new FieldGenerator(TypeBuilder, serviceInstanceType, GeneratorConstants.ServiceInstance);

            GenerateClassProxy(serviceType, serviceProviderGenerator, serviceInstanceGenerator);

            foreach (var impType in impInterfaceTypes)
            {
                GenerateInterfaceProxy(impType, serviceProviderGenerator, serviceInstanceGenerator);
            }

            return TypeBuilder.CreateTypeInfo();
        }

        protected override TypeBuilder GenerateTypeBuilder()
        {
            return moduleGenerator.CurrentModuleBuilder.DefineType($"{serviceType.Namespace}.{GeneratorConstants.Class}{serviceType.Name}", TypeAttributes.Class | TypeAttributes.Public, serviceType, Type.EmptyTypes);
        }

        public override Type GenerateProxyType()
        {
            return moduleGenerator.DefinedType(serviceType, key =>
            {
                return GenerateProxyTypeInfo(key).AsType();
            });
        }
    }
}
