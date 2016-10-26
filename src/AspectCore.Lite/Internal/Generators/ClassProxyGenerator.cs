using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internal.Generators
{
    internal sealed class ClassProxyGenerator : ProxyGenerator
    {
        public ClassProxyGenerator(IServiceProvider serviceProvider , Type parentType , params Type[] impInterfaceTypes)
            : base(serviceProvider , parentType , impInterfaceTypes)
        {
            ExceptionHelper.ThrowArgument(() => !parentType.GetTypeInfo().IsClass , $"Type {parentType} should be class." , nameof(parentType));
            ExceptionHelper.ThrowArgument(() => parentType.GetTypeInfo().IsSealed , $"Type {parentType} cannot be sealed." , nameof(parentType));
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
