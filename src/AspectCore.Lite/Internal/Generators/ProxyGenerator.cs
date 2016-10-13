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
        private readonly object syncobject = new object();
        private readonly Lazy<TypeBuilder> builder;
        internal readonly EmitBuilderProvider emitBuilderProvider;   
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
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (impInterfaceTypes == null)
            {
                throw new ArgumentNullException(nameof(impInterfaceTypes));
            }

            foreach (var impType in impInterfaceTypes)
            {
                if (!impType.GetTypeInfo().IsInterface)
                {
                    throw new ArgumentException($"Type {impType} should be interface.", nameof(impType));
                }
            }

            this.serviceType = serviceType;
            this.impInterfaceTypes = impInterfaceTypes;
            this.emitBuilderProvider = serviceProvider.GetRequiredService<EmitBuilderProvider>();
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

        protected void GenerateInterfaceProxy(Type serviceType, FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            foreach (var propertyInfo in serviceType.GetTypeInfo().DeclaredProperties)
            {
                var interfacePropertyGenerator = new PropertyGenerator(TypeBuilder, propertyInfo, serviceInstanceGenerator, serviceProviderGenerator);
                interfacePropertyGenerator.GenerateProperty();
            }

            foreach (var method in serviceType.GetTypeInfo().DeclaredMethods)
            {
                if (GeneratorUtilities.IsPropertyMethod(method, serviceType)) continue;
                var interfaceMethodGenerator = new InterfaceMethodGenerator(TypeBuilder, method, serviceInstanceGenerator, serviceProviderGenerator);
                interfaceMethodGenerator.GenerateMethod();
            }
        }
    }
}