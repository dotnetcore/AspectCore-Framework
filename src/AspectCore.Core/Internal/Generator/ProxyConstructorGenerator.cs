using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Internal;
using AspectCore.Abstractions.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class ProxyConstructorGenerator : ConstructorGenerator
    {
        protected readonly static Type[] ProxyParameterTypes = new Type[] { typeof(IServiceProvider), typeof(IServiceInstanceProvider) };

        protected readonly ConstructorInfo _constructor;
        protected readonly FieldBuilder _serviceInstanceFieldBuilder;
        protected readonly FieldBuilder _serviceProviderFieldBuilder;
        protected readonly Type _serviceType;

        public ProxyConstructorGenerator(TypeBuilder declaringMember,
            Type serviceType, ConstructorInfo constructor,
            FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
            : base(declaringMember)
        {
            _serviceType = serviceType;
            _constructor = constructor;
            _serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            _serviceProviderFieldBuilder = serviceProviderFieldBuilder;
        }

        public override CallingConventions CallingConventions
        {
            get
            {
                return _constructor.CallingConvention;
            }
        }

        public override MethodAttributes MethodAttributes
        {
            get
            {
                return _constructor.Attributes;
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                return new Type[] { typeof(IServiceProvider) }.Concat(_constructor.GetParameters().Select(p => p.ParameterType)).ToArray();
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
            ilGenerator.Emit(OpCodes.Call, _constructor);

            ilGenerator.EmitThis();
            ilGenerator.EmitLoadArg(1);
            ilGenerator.Emit(OpCodes.Stfld, _serviceProviderFieldBuilder);

            ilGenerator.EmitThis();
            ilGenerator.EmitThis();
            //ilGenerator.Emit(OpCodes.Ldnull);
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
            ilGenerator.Emit(OpCodes.Stfld, _serviceInstanceFieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}