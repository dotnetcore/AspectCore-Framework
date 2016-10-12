using System;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.Lite.Generators
{
    public sealed class InterfaceProxyGenerator
    {
        private readonly EmitBuilderProvider emitBuilderProvider;
        private readonly Type interfaceType;
        private TypeBuilder builder;

        public TypeBuilder TypeBuilder
        {
            get
            {
                if (builder == null) throw new InvalidOperationException($"The proxy of {interfaceType.FullName} is not generated.");
                return builder;
            }
        }

        public InterfaceProxyGenerator(IServiceProvider serviceProvider, Type interfaceType)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (!interfaceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException("Type should be interface.", nameof(interfaceType));
            }

            this.interfaceType = interfaceType;
            this.emitBuilderProvider = serviceProvider.GetRequiredService<EmitBuilderProvider>();
        }

        public Type GenerateProxyType()
        {
            return emitBuilderProvider.DefinedType(interfaceType, key =>
            {
                builder = emitBuilderProvider.CurrentModuleBuilder.DefineType($"{key.Namespace}.{GeneratorConstants.Interface}{key.Name}", TypeAttributes.Class | TypeAttributes.Public, typeof(object), new Type[] { key });

                var serviceProviderGenerator = new FieldGenerator(builder, typeof(IServiceProvider), GeneratorConstants.ServiceProvider);
                var serviceInstanceGenerator = new FieldGenerator(builder, key, GeneratorConstants.ServiceProvider);

                var constructorGenerator = new InterfaceConstructorGenerator(builder, key, serviceProviderGenerator, serviceInstanceGenerator);

                constructorGenerator.GenerateConstructor();

                foreach (var propertyInfo in key.GetTypeInfo().DeclaredProperties)
                {
                    var interfacePropertyGenerator = new PropertyGenerator(builder, propertyInfo, serviceInstanceGenerator, serviceProviderGenerator);
                    interfacePropertyGenerator.GenerateProperty();
                }

                foreach (var method in key.GetTypeInfo().DeclaredMethods)
                {
                    if (FilterPropertyMethod(method, key)) continue;
                    var interfaceMethodGenerator = new InterfaceMethodGenerator(builder, method, serviceInstanceGenerator, serviceProviderGenerator);
                    interfaceMethodGenerator.GenerateMethod();
                }

                return builder.CreateTypeInfo().AsType();
            });
        }

        private bool FilterPropertyMethod(MethodInfo method, Type serviceType) => 
            serviceType.GetTypeInfo().DeclaredProperties.Any(property => (property.CanRead && property.GetMethod == method) || (property.CanWrite && property.SetMethod == method));
        
    }
}
