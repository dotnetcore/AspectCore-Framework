using AspectCore.Lite.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internal.Generators
{
    internal abstract class ProxyGenerator
    {
        private readonly Lazy<TypeBuilder> builder;
        internal readonly ModuleGenerator moduleGenerator;
        protected readonly Type[] impInterfaceTypes;
        protected readonly Type serviceType;
        protected readonly IPointcut pointcut;

        public TypeBuilder TypeBuilder
        {
            get
            {
                return builder.Value;
            }
        }

        public ProxyGenerator(IServiceProvider serviceProvider, Type serviceType, Type[] impInterfaceTypes)
        {
            ExceptionHelper.ThrowArgumentNull(serviceProvider, nameof(serviceProvider));
            ExceptionHelper.ThrowArgumentNull(serviceType, nameof(serviceType));
            ExceptionHelper.ThrowArgumentNull(impInterfaceTypes, nameof(impInterfaceTypes));

            foreach (var impType in impInterfaceTypes)
            {
                ExceptionHelper.ThrowArgument(() => !impType.GetTypeInfo().IsInterface, $"Type {impType} should be interface.", nameof(impType));
            }

            this.serviceType = serviceType;
            this.impInterfaceTypes = impInterfaceTypes;
            this.moduleGenerator = serviceProvider.GetRequiredService<ModuleGenerator>();
            this.pointcut = serviceProvider.GetRequiredService<IPointcut>();
            this.builder = new Lazy<TypeBuilder>(() => GenerateTypeBuilder(), true);
        }

        protected abstract TypeBuilder GenerateTypeBuilder();

        public abstract Type GenerateProxyType();

        protected void GenerateClassProxy(Type parentType, FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            var constructorGenerator = new OverrideConstructorGenerator(TypeBuilder, parentType, serviceProviderGenerator, serviceInstanceGenerator);

            constructorGenerator.GenerateConstructor();

            foreach (var propertyInfo in parentType.GetTypeInfo().DeclaredProperties.Where(p =>
                    (p.CanRead && GeneratorHelper.IsOverridedMethod(p.GetMethod, pointcut) || (p.CanWrite && GeneratorHelper.IsOverridedMethod(p.SetMethod, pointcut)))))
            {
                var interfacePropertyGenerator = new OverridePropertyGenerator(TypeBuilder, propertyInfo, serviceInstanceGenerator, serviceProviderGenerator, pointcut);
                interfacePropertyGenerator.GenerateProperty();
            }

            foreach (var method in parentType.GetTypeInfo().DeclaredMethods.Where(m => GeneratorHelper.IsOverridedMethod(m, pointcut)))
            {
                if (GeneratorHelper.IsPropertyMethod(method, parentType)) continue;
                var methodGenerator = new OverrideMethodGenerator(TypeBuilder, method, serviceInstanceGenerator, serviceProviderGenerator, pointcut);
                methodGenerator.GenerateMethod();
            }
        }

        protected void GenerateInterfaceProxy(Type interfaceType, FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            foreach (var propertyInfo in interfaceType.GetTypeInfo().DeclaredProperties)
            {
                var interfacePropertyGenerator = new PropertyGenerator(TypeBuilder, propertyInfo, serviceInstanceGenerator, serviceProviderGenerator, pointcut);
                interfacePropertyGenerator.GenerateProperty();
            }

            foreach (var method in interfaceType.GetTypeInfo().DeclaredMethods)
            {
                if (GeneratorHelper.IsPropertyMethod(method, interfaceType)) continue;
                var interfaceMethodGenerator = new InterfaceMethodGenerator(TypeBuilder, method, serviceInstanceGenerator, serviceProviderGenerator, pointcut);
                interfaceMethodGenerator.GenerateMethod();
            }
        }
    }
}