using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Extensions;
using static AspectCore.Abstractions.Extensions.ReflectionExtensions;

namespace AspectCore.Abstractions.Internal.Generator
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

            ilGenerator.Emit(OpCodes.Call, _constructor);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(1);
            ilGenerator.Emit(OpCodes.Stfld, _serviceProviderFieldBuilder);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(2);
            if (_serviceType.GetTypeInfo().IsGenericTypeDefinition)
            {
                ilGenerator.EmitTypeof(_serviceType.GetTypeInfo().MakeGenericType(DeclaringMember.GetGenericArguments()));
            }
            else
            {
                ilGenerator.EmitTypeof(_serviceType);
            }
            ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ServiceInstanceProvider_GetInstance);
            ilGenerator.EmitConvertToType(typeof(object), _serviceType, false);
            ilGenerator.Emit(OpCodes.Stfld, _serviceInstanceFieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
