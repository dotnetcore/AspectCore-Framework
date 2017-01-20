using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal sealed class AspectTypeGenerator : TypeGenerator
    {
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
            if (!aspectValidator.Validate(serviceType))
            {
                throw new InvalidOperationException($"Validate '{serviceType}' failed because the type  does not satisfy the conditions of the generate proxy class.");
            }
            if (!parentType.GetTypeInfo().CanInherited())
            {
                throw new InvalidOperationException($"Validate '{parentType}' failed because the type does not satisfy the condition to be inherited.");
            }

            this.serviceType = serviceType;
            this.ParentType = parentType;
            this.aspectValidator = aspectValidator;
            if (this.aspectValidator == null)
            {
                throw new InvalidOperationException("No service for type 'AspectCore.Abstractions.IAspectValidator' has been registered.");
            }
        }

        public override Type[] Interfaces
        {
            get
            {
                return ParentType.GetTypeInfo().GetInterfaces();
            }
        }

        public override Type ParentType { get; }

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
            return ModuleGenerator.Default.DefineTypeInfo(TypeName, key => Build().CreateTypeInfo());
        }

        public Type CreateType()
        {
            return CreateTypeInfo().AsType();
        }

        private FieldBuilder serviceInstanceFieldBuilder;
        private FieldBuilder serviceProviderFieldBuilder;

        protected override TypeBuilder ExecuteBuild()
        {
            var builder = base.ExecuteBuild();
            if (ParentType.GetTypeInfo().IsGenericTypeDefinition)
            {
                GeneratingGenericParameter(builder);
            }
            var serviceInstanceFieldGenerator = new AspectFieldGenerator("__serviceInstance", serviceType, builder);
            var serviceProviderFieldGenerator = new AspectFieldGenerator("__serviceProvider", typeof(IServiceProvider), builder);

            serviceInstanceFieldBuilder = serviceInstanceFieldGenerator.Build();
            serviceProviderFieldBuilder = serviceProviderFieldGenerator.Build();

            GeneratingConstructor(builder, serviceInstanceFieldBuilder, serviceProviderFieldBuilder);
            GeneratingMethod(builder, serviceInstanceFieldBuilder, serviceProviderFieldBuilder);
            GeneratingProperty(builder, serviceInstanceFieldBuilder, serviceProviderFieldBuilder);

            builder.SetCustomAttribute(new CustomAttributeBuilder(typeof(NonAspectAttribute).GetTypeInfo().DeclaredConstructors.First(), EmptyArray<object>.Value));
            builder.SetCustomAttribute(new CustomAttributeBuilder(typeof(DynamicallyAttribute).GetTypeInfo().DeclaredConstructors.First(), EmptyArray<object>.Value));

            return builder;
        }

        private void GeneratingProperty(TypeBuilder builder, FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
        {
            foreach (var property in serviceType.GetTypeInfo().DeclaredProperties)
            {
                if (property.CanRead && aspectValidator.Validate(property.GetMethod))
                {
                    new AspectPropertyGenerator(builder, property, serviceType, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
                    continue;
                }
                if (property.CanWrite && aspectValidator.Validate(property.SetMethod))
                {
                    new AspectPropertyGenerator(builder, property, serviceType, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
                }
            }
        }

        private void GeneratingConstructor(TypeBuilder builder, FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
        {
            var constructors = ParentType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && c.IsPublic).ToArray();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"A suitable constructor for type {ParentType.FullName} could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
            }
            foreach (var constructor in constructors)
            {
                new AspectConstructorGenerator(builder, serviceType, constructor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
            }
        }

        private void GeneratingMethod(TypeBuilder builder, FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
        {
            foreach (var method in serviceType.GetTypeInfo().DeclaredMethods)
            {
                if (!aspectValidator.Validate(method))
                {
                    continue;
                }
                new AspectMethodGenerator(builder, serviceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
            }
        }

        private void GeneratingGenericParameter(TypeBuilder declaringMember)
        {
            var genericArguments = ParentType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = declaringMember.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }
    }
}
