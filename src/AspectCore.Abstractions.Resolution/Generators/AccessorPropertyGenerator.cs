using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal abstract class AccessorPropertyGenerator : PropertyGenerator
    {
        protected readonly FieldBuilder fieldBuilder;

        public AccessorPropertyGenerator(TypeBuilder declaringMember, FieldBuilder fieldBuilder) : base(declaringMember)
        {
            this.fieldBuilder = fieldBuilder;
        }

        public override CallingConventions CallingConventions { get; } = CallingConventions.HasThis;

        public override PropertyAttributes PropertyAttributes { get; } = PropertyAttributes.None;

        public override MethodInfo SetMethod
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanRead { get; } = true;

        public override bool CanWrite { get; } = false;

        protected override MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType)
        {
            return new AccessorPropertyMethodGenerator(declaringType, GetMethod, fieldBuilder);
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            throw new NotImplementedException();
        }

        internal static void Build(TypeBuilder declaringMember, FieldBuilder serviceProviderFieldBuilder, FieldBuilder serviceInstanceFieldBuilder)
        {
            new ServiceProviderAccessorPropertyGenerator(declaringMember, serviceProviderFieldBuilder).Build();
            new ServiceInstanceAccessorPropertyGenerator(declaringMember, serviceInstanceFieldBuilder).Build();
            new ServiceInstanceWithGenericAccessorPropertyGenerator(declaringMember, serviceInstanceFieldBuilder).Build();
        }

        private class AccessorPropertyMethodGenerator : GenericMethodGenerator
        {
            const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

            private readonly MethodInfo method;
            private readonly FieldBuilder fieldBuilder;

            public AccessorPropertyMethodGenerator(TypeBuilder declaringMember, MethodInfo method, FieldBuilder fieldBuilder) : base(declaringMember)
            {
                this.method = method;
                this.fieldBuilder = fieldBuilder;
            }

            public override CallingConventions CallingConventions => method.CallingConvention;

            public override bool IsGenericMethod => method.IsGenericMethod;

            public override MethodAttributes MethodAttributes { get; } = ExplicitMethodAttributes;

            public override string MethodName => method.GetFullName();

            public override Type[] ParameterTypes { get; } = Type.EmptyTypes;

            public override Type ReturnType => method.ReturnType;

            protected override MethodBuilder ExecuteBuild()
            {
                var builder = base.ExecuteBuild();

                DeclaringMember.DefineMethodOverride(builder, method);

                return builder;
            }

            protected override void GeneratingGenericParameter(MethodBuilder declaringMethod)
            {
                var genericArguments = method.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
                var genericArgumentsBuilders = declaringMethod.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
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

            protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
            {
                return new AccessorPropertyMethodBodyGenerator(declaringMethod, fieldBuilder);
            }
        }

        private class AccessorPropertyMethodBodyGenerator : MethodBodyGenerator
        {
            private readonly FieldBuilder fieldBuilder;

            public AccessorPropertyMethodBodyGenerator(MethodBuilder declaringMember, FieldBuilder fieldBuilder) : base(declaringMember)
            {
                this.fieldBuilder = fieldBuilder;
            }

            protected override void GeneratingMethodBody(ILGenerator ilGenerator)
            {
                ilGenerator.EmitThis();
                ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        private class ServiceProviderAccessorPropertyGenerator : AccessorPropertyGenerator
        {
            public ServiceProviderAccessorPropertyGenerator(TypeBuilder declaringMember, FieldBuilder fieldBuilder) : base(declaringMember, fieldBuilder)
            {
            }

            public override MethodInfo GetMethod { get; } = typeof(IServiceProviderAccessor).GetTypeInfo().GetMethod("get_ServiceProvider");

            public override string PropertyName { get; } = "AspectCore.Abstractions.IServiceProviderAccessor.ServiceProvider";

            public override Type PropertyType { get; } = typeof(IServiceProvider);
        }

        private class ServiceInstanceAccessorPropertyGenerator : AccessorPropertyGenerator
        {
            public ServiceInstanceAccessorPropertyGenerator(TypeBuilder declaringMember, FieldBuilder fieldBuilder) : base(declaringMember, fieldBuilder)
            {
            }

            public override MethodInfo GetMethod { get; } = typeof(IServiceInstanceAccessor).GetTypeInfo().GetMethod("get_ServiceInstance");

            public override string PropertyName { get; } = "AspectCore.Abstractions.IServiceInstanceAccessor.ServiceInstance";

            public override Type PropertyType { get; } = typeof(object);
        }

        private class ServiceInstanceWithGenericAccessorPropertyGenerator : AccessorPropertyGenerator
        {
            public ServiceInstanceWithGenericAccessorPropertyGenerator(TypeBuilder declaringMember, FieldBuilder fieldBuilder) : base(declaringMember, fieldBuilder)
            {
            }

            public override MethodInfo GetMethod => typeof(IServiceInstanceAccessor<>).MakeGenericType(PropertyType).GetTypeInfo().GetMethod("get_ServiceInstance");

            public override string PropertyName => $"'AspectCore.Abstractions.IServiceInstanceAccessor<{PropertyType.FullName}>.ServiceInstance'";

            public override Type PropertyType => fieldBuilder.FieldType;
        }
    }
}
