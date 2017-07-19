using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;

namespace AspectCore.Core.Internal.Generator
{
    internal class DefaultProxyConstructorGenerator : ProxyConstructorGenerator
    {
        private readonly static Type[] ProxyParameterTypes = new Type[] { typeof(IServiceProvider), typeof(IServiceInstanceProvider) };

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
            ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ServiceInstanceProviderGetInstance);
            ilGenerator.EmitConvertToType(typeof(object), _serviceType, false);
            ilGenerator.Emit(OpCodes.Stfld, _serviceInstanceFieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }

        protected override void GeneratingParameters(ConstructorBuilder constructorBuilder)
        {
            constructorBuilder.DefineParameter(1, ParameterAttributes.None, "serviceProvider");
            constructorBuilder.DefineParameter(2, ParameterAttributes.None, "serviceInstanceProvider");
        }
    }
}
