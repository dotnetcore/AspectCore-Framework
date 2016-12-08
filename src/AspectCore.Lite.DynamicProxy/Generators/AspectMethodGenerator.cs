using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using AspectCore.Lite.DynamicProxy.Common;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal sealed class AspectMethodGenerator : MethodGenerator
    {
        const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;

        private readonly Type serviceType;
        private readonly Type parentType;
        private readonly MethodInfo serviceMethod;
        private readonly FieldBuilder serviceInstanceFieldBuilder;
        private readonly FieldBuilder serviceProviderFieldBuilder;

        public AspectMethodGenerator(TypeBuilder declaringMember, Type serviceType, Type parentType, MethodInfo serviceMethod,
            FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder) : base(declaringMember)
        {
            this.serviceType = serviceType;
            this.parentType = parentType;
            this.serviceMethod = serviceMethod;
            this.serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            this.serviceInstanceFieldBuilder = serviceProviderFieldBuilder;
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
                return MethodHelper.ConvertMethodNameIfExplicit(serviceType, serviceMethod.Name);
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                return serviceMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            }
        }

        public override Type ReturnType
        {
            get
            {
                return serviceMethod.ReturnType;
            }
        }

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            var parentMethod = parentType.GetTypeInfo().GetMethod(serviceMethod.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return new AspectMethodBodyGenerator(declaringMethod, serviceType, parentType, serviceMethod, parentMethod ?? serviceMethod, serviceInstanceFieldBuilder, serviceProviderFieldBuilder);
        }
    }
}
