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
    public sealed class ClassProxyGenerator
    {
        private readonly EmitBuilderProvider emitBuilderProvider;
        private readonly Type parentType;
        private TypeBuilder builder;

        public TypeBuilder TypeBuilder
        {
            get
            {
                if (builder == null) throw new InvalidOperationException($"The proxy of {parentType.FullName} is not generated.");
                return builder;
            }
        }

        public ClassProxyGenerator(IServiceProvider serviceProvider, Type parentType)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (parentType == null)
            {
                throw new ArgumentNullException(nameof(parentType));
            }

            if (!parentType.GetTypeInfo().IsClass)
            {
                throw new ArgumentException($"Type {parentType} should be class.", nameof(parentType));
            }

            if(parentType.GetTypeInfo().IsSealed)
            {
                throw new ArgumentException($"Type {parentType} cannot be sealed.",nameof(parentType));
            }

            this.parentType = parentType;
            this.emitBuilderProvider = serviceProvider.GetRequiredService<EmitBuilderProvider>();
        }

        public Type GenerateProxyType()
        {
            return emitBuilderProvider.DefinedType(parentType, key =>
            {
                builder = emitBuilderProvider.CurrentModuleBuilder.DefineType($"{key.Namespace}.{GeneratorConstants.Class}{key.Name}", TypeAttributes.Class | TypeAttributes.Public, parentType, Type.EmptyTypes);

                var serviceProviderGenerator = new FieldGenerator(builder, typeof(IServiceProvider), GeneratorConstants.ServiceProvider);
                var serviceInstanceGenerator = new FieldGenerator(builder, key, GeneratorConstants.ServiceInstance);

                var constructorGenerator = new OverrideConstructorGenerator(builder, key, serviceProviderGenerator, serviceInstanceGenerator);

                constructorGenerator.GenerateConstructor();

                var pointcut = PointcutUtilities.GetPointcut(key.GetTypeInfo());

                foreach (var propertyInfo in key.GetTypeInfo().DeclaredProperties.Where(p =>
                        (p.CanRead && pointcut.IsMatch(p.GetMethod)) || (p.CanWrite && pointcut.IsMatch(p.SetMethod))))
                {
                    var interfacePropertyGenerator = new PropertyGenerator(builder, propertyInfo, serviceInstanceGenerator, serviceProviderGenerator);
                    interfacePropertyGenerator.GenerateProperty();
                }

                foreach (var method in key.GetTypeInfo().DeclaredMethods.Where(m=> pointcut.IsMatch(m)))
                {
                    if (GeneratorUtilities.IsPropertyMethod(method, key)) continue;
                    var interfaceMethodGenerator = new InterfaceMethodGenerator(builder, method, serviceInstanceGenerator, serviceProviderGenerator);
                    interfaceMethodGenerator.GenerateMethod();
                }

                return builder.CreateTypeInfo().AsType();
            });
        }
    }
}
