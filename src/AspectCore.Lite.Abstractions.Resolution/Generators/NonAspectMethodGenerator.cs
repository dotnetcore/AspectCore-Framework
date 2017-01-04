using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Resolution.Generators
{
    internal sealed class NonAspectMethodGenerator : GenericMethodGenerator
    {
        const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        private readonly MethodInfo serviceMethod;
        private readonly FieldBuilder serviceInstanceFieldBuilder;

        public NonAspectMethodGenerator(TypeBuilder declaringMember, MethodInfo serviceMethod, FieldBuilder serviceInstanceFieldBuilder)
            : base(declaringMember)
        {
            this.serviceMethod = serviceMethod;
            this.serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
        }

        public override CallingConventions CallingConventions { get; } = CallingConventions.Standard;

        public override MethodAttributes MethodAttributes { get; } = ExplicitMethodAttributes;

        public override string MethodName
        {
            get
            {
                return MethodHelper.ConvertMethodNameIfExplicit(serviceMethod.DeclaringType, serviceMethod.Name);
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

        public override bool IsGenericMethod
        {
            get
            {
                return serviceMethod.IsGenericMethod;
            }
        }

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            return new NonAspectMethodBodyGenerator(declaringMethod, serviceMethod, serviceInstanceFieldBuilder);
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
