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
    internal sealed class NonAspectMethodGenerator : MethodGenerator
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

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            return new NonAspectMethodBodyGenerator(declaringMethod, serviceMethod, serviceInstanceFieldBuilder);
        }
    }
}
