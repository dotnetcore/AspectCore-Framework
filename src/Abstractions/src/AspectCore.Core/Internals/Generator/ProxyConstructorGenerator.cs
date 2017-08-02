using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;
using AspectCore.Core.Generator;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Core.Internal.Generator
{
    internal class ProxyConstructorGenerator : ConstructorGenerator
    {
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

        protected override ConstructorBuilder ExecuteBuild()
        {
            var builder = base.ExecuteBuild();

            GeneratingParameters(builder);

            GeneratingCustomAttribute(builder);

            return builder;
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
            ilGenerator.Emit(OpCodes.Stfld, _serviceInstanceFieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }

        protected virtual void GeneratingCustomAttribute(ConstructorBuilder constructorBuilder)
        {
            constructorBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(DynamicallyAttribute).GetTypeInfo().DeclaredConstructors.First(), EmptyArray<object>.Value));
            foreach(var attribute in _constructor.CustomAttributes)
            {
                new ConstructorAttributeBuilder(constructorBuilder, attribute).Build();
            }
        }

        protected virtual void GeneratingParameters(ConstructorBuilder constructorBuilder)
        {
            constructorBuilder.DefineParameter(1, ParameterAttributes.None, "serviceProvider");
            var parameters = _constructor.GetParameters();
            if (parameters.Length > 0)
            {
                var paramOffset = 2;    //ParameterTypes.Length - parameters.Length + 1
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var parameterBuilder = constructorBuilder.DefineParameter(i + paramOffset, parameter.Attributes, parameter.Name);
                    if (parameter.HasDefaultValue)
                    {
                        parameterBuilder.SetConstant(parameter.DefaultValue);
                    }
                    new ParameterAttributeBuilder(parameterBuilder, typeof(DynamicallyAttribute)).Build();
                    foreach (var attribute in parameter.CustomAttributes)
                    {
                        new ParameterAttributeBuilder(parameterBuilder, attribute).Build();
                    }
                }
            }
        }
    }
}