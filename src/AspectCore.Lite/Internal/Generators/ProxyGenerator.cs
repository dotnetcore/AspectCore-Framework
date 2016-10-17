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
    public abstract class ProxyGenerator
    {
        private readonly Lazy<TypeBuilder> builder;
        internal readonly ModuleGenerator moduleGenerator;   
        protected readonly Type[] impInterfaceTypes;
        protected readonly Type serviceType;

        public TypeBuilder TypeBuilder
        {
            get
            {    
                return builder.Value;
            }
        }

        public ProxyGenerator(IServiceProvider serviceProvider, Type serviceType, Type[] impInterfaceTypes)
        {
            ExceptionUtilities.ThrowArgumentNull(serviceProvider , nameof(serviceProvider));
            ExceptionUtilities.ThrowArgumentNull(serviceType , nameof(serviceType));
            ExceptionUtilities.ThrowArgumentNull(impInterfaceTypes , nameof(impInterfaceTypes));

            foreach (var impType in impInterfaceTypes)
            {
                ExceptionUtilities.ThrowArgument(() => !impType.GetTypeInfo().IsInterface , $"Type {impType} should be interface." , nameof(impType));
            }

            this.serviceType = serviceType;
            this.impInterfaceTypes = impInterfaceTypes;
            this.moduleGenerator = serviceProvider.GetRequiredService<ModuleGenerator>();
            this.builder = new Lazy<TypeBuilder>(() => GenerateTypeBuilder(), true);
        }

        protected abstract TypeBuilder GenerateTypeBuilder();

        public abstract Type GenerateProxyType();

        protected void GenerateClassProxy(Type parentType, FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            var constructorGenerator = new OverrideConstructorGenerator(TypeBuilder, parentType, serviceProviderGenerator, serviceInstanceGenerator);

            constructorGenerator.GenerateConstructor();

            foreach (var propertyInfo in parentType.GetTypeInfo().DeclaredProperties.Where(p =>
                    (p.CanRead && GeneratorUtilities.IsOverridedMethod(p.GetMethod, parentType) || (p.CanWrite && GeneratorUtilities.IsOverridedMethod(p.SetMethod, parentType)))))
            {
                var interfacePropertyGenerator = new OverridePropertyGenerator(TypeBuilder, propertyInfo, serviceInstanceGenerator, serviceProviderGenerator);
                interfacePropertyGenerator.GenerateProperty();
            }

            foreach (var method in parentType.GetTypeInfo().DeclaredMethods.Where(m => GeneratorUtilities.IsOverridedMethod(m, parentType)))
            {
                if (GeneratorUtilities.IsPropertyMethod(method, parentType)) continue;
                var methodGenerator = new OverrideMethodGenerator(TypeBuilder, method, serviceInstanceGenerator, serviceProviderGenerator);
                methodGenerator.GenerateMethod();
            }
        }

        protected void GenerateInterfaceProxy(Type interfaceType, FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            foreach (var propertyInfo in interfaceType.GetTypeInfo().DeclaredProperties)
            {
                var interfacePropertyGenerator = new PropertyGenerator(TypeBuilder, propertyInfo, serviceInstanceGenerator, serviceProviderGenerator);
                interfacePropertyGenerator.GenerateProperty();
            }

            foreach (var method in interfaceType.GetTypeInfo().DeclaredMethods)
            {
                if (GeneratorUtilities.IsPropertyMethod(method, interfaceType)) continue;
                var interfaceMethodGenerator = new InterfaceMethodGenerator(TypeBuilder, method, serviceInstanceGenerator, serviceProviderGenerator);
                interfaceMethodGenerator.GenerateMethod();
            }
        }
    }
}