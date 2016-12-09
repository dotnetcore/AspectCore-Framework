using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Generator;
using AspectCore.Lite.DynamicProxy.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal sealed class AspectTypeGenerator : TypeGenerator
    {
        private readonly Lazy<TypeBuilder> builder;
        private readonly Type serviceType;
        private readonly IServiceProvider serviceProvider;
        private readonly IAspectValidator aspectValidator;

        public AspectTypeGenerator(Type serviceType, Type parentType, IServiceProvider serviceProvider) : base(ModuleGenerator.Default.ModuleBuilder)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (parentType == null)
            {
                throw new ArgumentNullException(nameof(parentType));
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.serviceType = serviceType;
            this.ParentType = parentType;
            this.builder = new Lazy<TypeBuilder>(InitializeTypeBuilder);
            this.serviceProvider = serviceProvider;
            this.aspectValidator = (IAspectValidator)serviceProvider.GetService(typeof(IAspectValidator));
            if (this.aspectValidator == null)
            {
                throw new InvalidOperationException("No service for type 'AspectCore.Lite.Abstractions.IAspectValidator' has been registered.");
            }
        }

        public override Type[] Interfaces { get; } = System.Type.EmptyTypes;

        public override Type ParentType { get; }

        public override TypeBuilder Type
        {
            get { return builder.Value; }
            protected set { }
        }

        public override TypeAttributes TypeAttributes { get; } = TypeAttributes.Class | TypeAttributes.Public;

        public override string TypeName
        {
            get
            {
                return $"{serviceType.Namespace}.Proxy{ParentType.Name}As{serviceType.Name}";
            }
        }

        public override TypeInfo CreateTypeInfo()
        {
            return ModuleGenerator.Default.DefineTypeInfo(TypeName, key => Accept(new AspectGeneratorVisitor()).CreateTypeInfo());
        }

        public Type CreateType()
        {
            return CreateTypeInfo().AsType();
        }

        protected override TypeBuilder Accept(GeneratorVisitor visitor)
        {
            var serviceInstanceFieldGenerator = new AspectFieldGenerator("proxyfield_serviceInstance", serviceType, Type);
            var serviceProviderFieldGenerator = new AspectFieldGenerator("proxyfield_serviceProvider", typeof(IServiceProvider), Type);
            var serviceInstanceFieldBuilder = (FieldBuilder)visitor.VisitGenerator(serviceInstanceFieldGenerator);
            var serviceProviderFieldBuilder = (FieldBuilder)visitor.VisitGenerator(serviceProviderFieldGenerator);

            GeneratingConstructor(serviceInstanceFieldBuilder, serviceProviderFieldBuilder);

            GeneratingMethod(serviceInstanceFieldBuilder, serviceProviderFieldBuilder);

            return base.Accept(visitor);
        }

        private void GeneratingConstructor(FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
        {
            var constructors = ParentType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && c.IsPublic).ToArray();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"A suitable constructor for type {ParentType.FullName} could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
            }
            foreach(var constructor in constructors)
            {
                AddMember(new AspectConstructorGenerator(Type, serviceType, constructor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder));
            }
        }

        private void GeneratingMethod(FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
        {
            foreach (var method in serviceType.GetTypeInfo().DeclaredMethods)
            {
                if (method.IsPropertyMethod()) continue;
                if (!aspectValidator.Validate(method))
                {
                    if (serviceType.GetTypeInfo().IsInterface)
                    {
                        AddMember(new NonAspectMethodGenerator(Type, method, serviceInstanceFieldBuilder));
                    }
                    continue;
                }
                AddMember(new AspectMethodGenerator(Type, serviceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder));
            }
        }
    }
}
