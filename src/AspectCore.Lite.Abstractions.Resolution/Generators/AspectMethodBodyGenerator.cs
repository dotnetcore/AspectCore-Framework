using AspectCore.Lite.Abstractions.Common;
using AspectCore.Lite.Abstractions.Generator;
using AspectCore.Lite.Abstractions.Resolution.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Resolution.Generators
{
    internal sealed class AspectMethodBodyGenerator : MethodBodyGenerator
    {
        private readonly Type serviceType;
        private readonly Type parentType;
        private readonly MethodInfo serviceMethod;
        private readonly MethodInfo parentMethod;
        private readonly FieldBuilder serviceInstanceFieldBuilder;
        private readonly FieldBuilder serviceProviderFieldBuilder;
        private readonly TypeBuilder declaringBuilder;

        public AspectMethodBodyGenerator(MethodBuilder declaringMember, TypeBuilder declaringBuilder,
            Type serviceType, Type parentType,
            MethodInfo serviceMethod, MethodInfo parentMethod,
            FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder)
            : base(declaringMember)
        {
            this.serviceType = serviceType;
            this.parentType = parentType;
            this.serviceMethod = serviceMethod;
            this.parentMethod = parentMethod;
            this.serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            this.serviceProviderFieldBuilder = serviceProviderFieldBuilder;
            this.declaringBuilder = declaringBuilder;
        }

        protected override void GeneratingMethodBody(ILGenerator ilGenerator)
        {
            var parameters = serviceMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, serviceProviderFieldBuilder);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GETASPECTACTIVATOR);  //var aspectActivator = this.serviceProvider.GetService<IAspectActivator>();
            ilGenerator.Emit(OpCodes.Dup);
            GeneratingInitializeMetaData(ilGenerator);           
            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, serviceInstanceFieldBuilder);
            ilGenerator.EmitThis();
            ilGenerator.EmitLoadInt(parameters.Length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameters.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.EmitLoadInt(i);
                ilGenerator.EmitLoadArg(i + 1);
                ilGenerator.EmitConvertToType(parameters[i], typeof(object), false);
                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }
            GeneratingReturnVaule(ilGenerator);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private void GeneratingInitializeMetaData(ILGenerator ilGenerator)
        {
            if (serviceType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var serviceTypeOfGeneric = serviceType.GetTypeInfo().MakeGenericType(declaringBuilder.GetGenericArguments());
                ilGenerator.EmitTypeof(serviceTypeOfGeneric);
            }
            else
            {
                ilGenerator.EmitTypeof(serviceType);
            }

            if (serviceMethod.IsGenericMethodDefinition)
            {
                ilGenerator.EmitMethodof(serviceMethod.MakeGenericMethod(DeclaringMember.GetGenericArguments()));
                ilGenerator.EmitMethodof(parentMethod.MakeGenericMethod(DeclaringMember.GetGenericArguments()));
                ilGenerator.EmitMethodof(DeclaringMember.MakeGenericMethod(DeclaringMember.GetGenericArguments()));
            }
            else
            {
                ilGenerator.EmitMethodof(serviceMethod);
                ilGenerator.EmitMethodof(parentMethod);
                ilGenerator.EmitMethodof(DeclaringMember);
            }

            ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ASPECTACTIVATOR_INITIALIZEMETADATA);
        }

        private void GeneratingReturnVaule(ILGenerator ilGenerator)
        {

            if (serviceMethod.ReturnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ASPECTACTIVATOR_INVOKE.MakeGenericMethod(typeof(object)));
                ilGenerator.Emit(OpCodes.Pop);
            }
            else if (serviceMethod.ReturnType == typeof(Task))
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ASPECTACTIVATOR_INVOKEASYNC.MakeGenericMethod(typeof(object)));
            }
            else if (serviceMethod.IsReturnTask())
            {
                var returnType = serviceMethod.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ASPECTACTIVATOR_INVOKEASYNC.MakeGenericMethod(returnType));
            }
            else
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodInfoConstant.ASPECTACTIVATOR_INVOKE.MakeGenericMethod(serviceMethod.ReturnType));
            }
        }
    }
}
