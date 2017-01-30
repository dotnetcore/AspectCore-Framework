using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using AspectCore.Abstractions.Resolution.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class ProxyConstructorGenerator : ConstructorGenerator
    {
        protected readonly static Type[] ProxyParameterTypes = new Type[] { typeof(IServiceProvider), typeof(IServiceInstanceProvider) };

        protected readonly ConstructorInfo constructor;
        protected readonly FieldBuilder serviceInstanceFieldBuilder;
        protected readonly FieldBuilder serviceProviderFieldBuilder;
        protected readonly Type serviceType;

        public ProxyConstructorGenerator(TypeBuilder declaringMember,
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
                return new Type[] { typeof(IServiceProvider) }.Concat(constructor.GetParameters().Select(p => p.ParameterType)).ToArray();
            }
        }

        protected override void GeneratingConstructorBody(ILGenerator ilGenerator)
        {
            var parameters = ParameterTypes;

            ilGenerator.EmitThis();
            for (var i = 2; i <= parameters.Length; i++)
            {
                ilGenerator.EmitLoadArg(i);
            }
            ilGenerator.Emit(OpCodes.Call, constructor);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(1);
            ilGenerator.Emit(OpCodes.Stfld, serviceProviderFieldBuilder);

            ilGenerator.EmitThis();
            ilGenerator.EmitThis();
            //ilGenerator.EmitLoadArg(2);
            //if (serviceType.GetTypeInfo().IsGenericTypeDefinition)
            //{
            //    ilGenerator.EmitTypeof(serviceType.GetTypeInfo().MakeGenericType(DeclaringMember.GetGenericArguments()));
            //}
            //else
            //{
            //    ilGenerator.EmitTypeof(serviceType);
            //}
            //ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ServiceInstanceProvider_GetInstance);
            //ilGenerator.EmitConvertToType(typeof(object), serviceType, false);
            ilGenerator.Emit(OpCodes.Stfld, serviceInstanceFieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}