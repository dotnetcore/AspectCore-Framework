using AspectCore.Lite.Abstractions.Extensions;
using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Resolution.Generators
{
    internal sealed class AspectConstructorGenerator : ConstructorGenerator
    {
        private readonly ConstructorInfo constructor;
        private readonly FieldBuilder serviceInstanceFieldBuilder;
        private readonly FieldBuilder serviceProviderFieldBuilder;
        private readonly Type serviceType;

        public AspectConstructorGenerator(TypeBuilder declaringMember,
            Type serviceType, ConstructorInfo constructor, 
            FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
            : base(declaringMember)
        {
            this.serviceType = serviceType;
            this.constructor = constructor;
            this.serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            this.serviceProviderFieldBuilder = serviceProviderFieldBuilder;
        }

        public override CallingConventions CallingConventions
        {
            get
            {
                return constructor.CallingConvention;
            }
        }

        public override MethodAttributes MethodAttributes
        {
            get
            {
                return constructor.Attributes;
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                var serviceTypeParameters = new Type[] { typeof(IServiceProvider), typeof(IOriginalServiceProvider) };
                return constructor.GetParameters().Select(p => p.ParameterType).Concat(serviceTypeParameters).ToArray();
            }
        }

        protected override void GeneratingConstructorBody(ILGenerator ilGenerator)
        {
            var parameters = ParameterTypes;

            ilGenerator.EmitThis();
            for (var i = 1; i <= parameters.Length - 2; i++)
            {
                ilGenerator.EmitLoadArg(i);
            }
            ilGenerator.Emit(OpCodes.Call, constructor);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(parameters.Length - 1);
            ilGenerator.Emit(OpCodes.Stfld, serviceProviderFieldBuilder);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(parameters.Length);
            if (serviceType.GetTypeInfo().IsGenericTypeDefinition)
            {
                ilGenerator.EmitTypeof(serviceType.GetTypeInfo().MakeGenericType(DeclaringMember.GetGenericArguments()));
            }
            else
            {
                ilGenerator.EmitTypeof(serviceType);
            }
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.SupportOriginalService_GetService);
            ilGenerator.EmitConvertToType(typeof(object), serviceType, false);
            ilGenerator.Emit(OpCodes.Stfld, serviceInstanceFieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}