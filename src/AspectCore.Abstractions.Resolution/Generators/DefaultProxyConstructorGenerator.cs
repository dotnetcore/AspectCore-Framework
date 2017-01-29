using AspectCore.Abstractions.Extensions;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class DefaultProxyConstructorGenerator : ProxyConstructorGenerator
    {
        public DefaultProxyConstructorGenerator(TypeBuilder declaringMember, Type serviceType, ConstructorInfo constructor, FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder) : base(declaringMember, serviceType, constructor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder)
        {
        }

        public override Type[] ParameterTypes { get; } = ProxyParameterTypes;

        protected override void GeneratingConstructorBody(ILGenerator ilGenerator)
        {
            ilGenerator.EmitThis();

            ilGenerator.Emit(OpCodes.Call, constructor);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(1);
            ilGenerator.Emit(OpCodes.Stfld, serviceProviderFieldBuilder);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(2);
            if (serviceType.GetTypeInfo().IsGenericTypeDefinition)
            {
                ilGenerator.EmitTypeof(serviceType.GetTypeInfo().MakeGenericType(DeclaringMember.GetGenericArguments()));
            }
            else
            {
                ilGenerator.EmitTypeof(serviceType);
            }
            ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ServiceInstanceProvider_GetInstance);
            ilGenerator.EmitConvertToType(typeof(object), serviceType, false);
            ilGenerator.Emit(OpCodes.Stfld, serviceInstanceFieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
