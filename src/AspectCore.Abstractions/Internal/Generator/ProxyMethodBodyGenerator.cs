using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using static AspectCore.Abstractions.Extensions.ReflectionExtensions;

namespace AspectCore.Abstractions.Internal.Generator
{
    internal sealed class ProxyMethodBodyGenerator : MethodBodyGenerator
    {
        private readonly Type _serviceType;
        private readonly MethodInfo _serviceMethod;
        private readonly MethodInfo _parentMethod;
        private readonly FieldBuilder _serviceInstanceFieldBuilder;
        private readonly FieldBuilder _serviceProviderFieldBuilder;
        private readonly TypeBuilder _declaringBuilder;

        public ProxyMethodBodyGenerator(
                MethodBuilder declaringMember, 
                TypeBuilder declaringBuilder,
                Type serviceType, 
                MethodInfo serviceMethod, 
                MethodInfo parentMethod,
                FieldBuilder serviceInstanceFieldBuilder, 
                FieldBuilder serviceProviderFieldBuilder)
                : base(declaringMember)
        {
            _serviceType = serviceType;
            _serviceMethod = serviceMethod;
            _parentMethod = parentMethod;
            _serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            _serviceProviderFieldBuilder = serviceProviderFieldBuilder;
            _declaringBuilder = declaringBuilder;
        }

        protected override void GeneratingMethodBody(ILGenerator ilGenerator)
        {
            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, _serviceProviderFieldBuilder);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetAspectActivator);  //var aspectActivator = this.serviceProvider.GetService<IAspectActivator>();      
            GeneratingInitializeMetaData(ilGenerator);
            ilGenerator.Emit(OpCodes.Newobj, MethodInfoConstant.AspectActivatorContex_Ctor);
            GeneratingReturnVaule(ilGenerator);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private void GeneratingInitializeMetaData(ILGenerator ilGenerator)
        {
            if (_serviceType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var serviceTypeOfGeneric = _serviceType.GetTypeInfo().MakeGenericType(_declaringBuilder.GetGenericArguments());
                ilGenerator.EmitTypeof(serviceTypeOfGeneric);
            }
            else
            {
                ilGenerator.EmitTypeof(_serviceType);
            }

            if (_serviceMethod.IsGenericMethodDefinition)
            {
                ilGenerator.EmitMethodof(_serviceMethod.MakeGenericMethod(DeclaringMember.GetGenericArguments()));
                ilGenerator.EmitMethodof(_parentMethod.MakeGenericMethod(DeclaringMember.GetGenericArguments()));
                ilGenerator.EmitMethodof(DeclaringMember.MakeGenericMethod(DeclaringMember.GetGenericArguments()));
            }
            else
            {
                ilGenerator.EmitMethodof(_serviceMethod);
                ilGenerator.EmitMethodof(_parentMethod);
                ilGenerator.EmitMethodof(DeclaringMember);
            }

            var parameters = _serviceMethod.GetParameterTypes();

            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, _serviceInstanceFieldBuilder);
            ilGenerator.EmitThis();
            ilGenerator.EmitLoadInt(parameters.Length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameters.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.EmitLoadInt(i);
                ilGenerator.EmitLoadArg(i + 1);
                ilGenerator.EmitConvertToObject(parameters[i]);
                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }

        }

        private void GeneratingReturnVaule(ILGenerator ilGenerator)
        {
            if (_serviceMethod.ReturnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivator_Invoke.MakeGenericMethod(typeof(object)));
                ilGenerator.Emit(OpCodes.Pop);
            }
            else if (_serviceMethod.ReturnType == typeof(Task))
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivator_InvokeAsync.MakeGenericMethod(typeof(object)));
            }
            else if (_serviceMethod.IsReturnTask())
            {
                var returnType = _serviceMethod.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivator_InvokeAsync.MakeGenericMethod(returnType));
            }
            else
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivator_Invoke.MakeGenericMethod(_serviceMethod.ReturnType));
            }
        }
    }
}
