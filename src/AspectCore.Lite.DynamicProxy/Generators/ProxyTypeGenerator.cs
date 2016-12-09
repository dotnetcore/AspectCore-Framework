using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    public class ProxyTypeGenerator : TypeGenerator
    {   
        private readonly Lazy<TypeBuilder> builder;
        private readonly Type serviceType;
        private readonly IServiceProvider serviceProvider;
        private readonly IAspectValidator aspectValidator;

        public ProxyTypeGenerator(Type serviceType, Type parentType, IServiceProvider serviceProvider) : base(ModuleGenerator.Default.ModuleBuilder)
        {
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
            return ModuleGenerator.Default.DefineTypeInfo(TypeName, key => base.CreateTypeInfo());
        }

        protected override TypeBuilder Accept(GeneratorVisitor visitor)
        {
            var serviceInstanceFieldGenerator = new PrivateFieldGenerator("proxyfield_serviceInstance", serviceType, Type);
            var serviceProviderFieldGenerator = new PrivateFieldGenerator("proxyfield_serviceProvider", typeof(IServiceProvider), Type);
            var serviceInstanceFieldBuilder = (FieldBuilder)visitor.VisitGenerator(serviceInstanceFieldGenerator);
            var serviceProviderFieldBuilder = (FieldBuilder)visitor.VisitGenerator(serviceProviderFieldGenerator);

            return base.Accept(visitor);
        }

    }
}
