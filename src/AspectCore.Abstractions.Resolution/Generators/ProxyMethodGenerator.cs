using AspectCore.Abstractions.Generator;
using AspectCore.Abstractions.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class ProxyMethodGenerator : GenericMethodGenerator
    {
        const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;

        protected readonly Type serviceType;
        protected readonly Type parentType;
        protected readonly MethodInfo serviceMethod;
        protected readonly FieldBuilder serviceInstanceFieldBuilder;
        protected readonly FieldBuilder serviceProviderFieldBuilder;
        protected readonly bool isImplementExplicitly;

        public ProxyMethodGenerator(TypeBuilder declaringMember, Type serviceType, Type parentType, MethodInfo serviceMethod,
            FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder, bool isImplementExplicitly) : base(declaringMember)
        {
            this.serviceType = serviceType;
            this.parentType = parentType;
            this.serviceMethod = serviceMethod;
            this.serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            this.serviceProviderFieldBuilder = serviceProviderFieldBuilder;
            this.isImplementExplicitly = isImplementExplicitly;
        }

        public override CallingConventions CallingConventions
        {
            get
            {
                return serviceMethod.CallingConvention;
            }
        }

        public override MethodAttributes MethodAttributes
        {
            get
            {
                if (serviceType.GetTypeInfo().IsInterface)
                {
                    return ExplicitMethodAttributes;
                }

                var attributes = OverrideMethodAttributes;

                if (serviceMethod.Attributes.HasFlag(MethodAttributes.Public))
                {
                    attributes = attributes | MethodAttributes.Public;
                }

                if (serviceMethod.Attributes.HasFlag(MethodAttributes.Family))
                {
                    attributes = attributes | MethodAttributes.Family;
                }

                if (serviceMethod.Attributes.HasFlag(MethodAttributes.FamORAssem))
                {
                    attributes = attributes | MethodAttributes.FamORAssem;
                }

                return attributes;
            }
        }

        public override string MethodName
        {
            get
            {
                return isImplementExplicitly ? serviceMethod.GetFullName() : serviceMethod.Name;
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                return serviceMethod.GetParameterTypes();
            }
        }

        public override Type ReturnType
        {
            get
            {
                return serviceMethod.ReturnType;
            }
        }

        public override bool IsGenericMethod
        {
            get
            {
                return serviceMethod.IsGenericMethod;
            }
        }

        protected override MethodBuilder ExecuteBuild()
        {
            var builder = base.ExecuteBuild();

            if (serviceType.GetTypeInfo().IsInterface)
            {
                DeclaringMember.DefineMethodOverride(builder, serviceMethod);
            }

            return builder;
        }

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            var parentMethod = parentType.GetTypeInfo().GetMethod(serviceMethod.Name, serviceMethod.GetParameterTypes());
            return new ProxyMethodBodyGenerator(declaringMethod,
                DeclaringMember,
                serviceType,
                serviceMethod,
                parentMethod ?? serviceMethod,
                serviceInstanceFieldBuilder,
                serviceProviderFieldBuilder);
        }

        protected override void GeneratingGenericParameter(MethodBuilder declaringMethod)
        {
            var genericArguments = serviceMethod.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
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
    }
}
