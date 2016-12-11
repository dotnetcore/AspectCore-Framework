using AspectCore.Lite.Abstractions.Generator;
using AspectCore.Lite.Abstractions.Resolution.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Resolution.Generators
{
    internal sealed class AspectTypeGenerator : TypeGenerator
    {
        private readonly Lazy<TypeBuilder> builder;
        private readonly Type serviceType;
        private readonly IAspectValidator aspectValidator;

        public AspectTypeGenerator(Type serviceType, Type parentType, IAspectValidator aspectValidator) : base(ModuleGenerator.Default.ModuleBuilder)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (parentType == null)
            {
                throw new ArgumentNullException(nameof(parentType));
            }
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }

            this.serviceType = serviceType;
            this.ParentType = parentType;
            this.builder = new Lazy<TypeBuilder>(InitializeTypeBuilder);
            this.aspectValidator = aspectValidator;
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
        private FieldBuilder serviceInstanceFieldBuilder;
        private FieldBuilder serviceProviderFieldBuilder;

        protected override TypeBuilder Accept(GeneratorVisitor visitor)
        {
            var serviceInstanceFieldGenerator = new AspectFieldGenerator("proxyfield_serviceInstance", serviceType, Type);
            var serviceProviderFieldGenerator = new AspectFieldGenerator("proxyfield_serviceProvider", typeof(IServiceProvider), Type);
            serviceInstanceFieldBuilder = (FieldBuilder)visitor.VisitGenerator(serviceInstanceFieldGenerator);
            serviceProviderFieldBuilder = (FieldBuilder)visitor.VisitGenerator(serviceProviderFieldGenerator);

            GeneratingConstructor(serviceInstanceFieldBuilder, serviceProviderFieldBuilder);

            GeneratingMethod(serviceInstanceFieldBuilder, serviceProviderFieldBuilder);

            Type.SetCustomAttribute(new CustomAttributeBuilder(typeof(NonAspectAttribute).GetTypeInfo().DeclaredConstructors.First(), null));

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
