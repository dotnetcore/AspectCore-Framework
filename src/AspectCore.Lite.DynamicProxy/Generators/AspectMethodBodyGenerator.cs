using AspectCore.Lite.Abstractions.Generator;
using AspectCore.Lite.DynamicProxy.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal sealed class AspectMethodBodyGenerator : MethodBodyGenerator
    {
        private readonly Type serviceType;
        private readonly Type parentType;
        private readonly MethodInfo serviceMethod;
        private readonly MethodInfo parentMethod;
        private readonly FieldBuilder serviceInstanceFieldBuilder;
        private readonly FieldBuilder serviceProviderFieldBuilder;

        public AspectMethodBodyGenerator(MethodBuilder declaringMember,
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
        }

        protected override void GeneratingMethodBody(ILGenerator ilGenerator)
        {
            var parameters = serviceMethod.GetParameters().Select(p => p.ParameterType).ToArray();

            ilGenerator.EmitThis();
            ilGenerator.Emit(OpCodes.Ldfld, serviceProviderFieldBuilder);
            ilGenerator.Emit(OpCodes.Call, MethodConstant.GetAspectActivator);  //var aspectActivator = this.serviceProvider.GetService<IAspectActivator>();

            ilGenerator.Emit(OpCodes.Dup);

            ilGenerator.EmitTypeof(serviceType);
            ilGenerator.EmitMethodof(serviceMethod);
            ilGenerator.EmitMethodof(parentMethod);
            ilGenerator.EmitMethodof(DeclaringMember);
            ilGenerator.Emit(OpCodes.Callvirt, MethodConstant.AspectActivator_InitializeMetaData);

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

            if (serviceMethod.ReturnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodConstant.AspectActivator_Invoke.MakeGenericMethod(typeof(object)));
                ilGenerator.Emit(OpCodes.Pop);
            }
            else if (serviceMethod.ReturnType == typeof(Task))
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodConstant.AspectActivator_InvokeAsync.MakeGenericMethod(typeof(object)));
            }
            else if (serviceMethod.IsReturnTask())
            {
                var returnType = serviceMethod.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                ilGenerator.Emit(OpCodes.Callvirt, MethodConstant.AspectActivator_InvokeAsync.MakeGenericMethod(returnType));
            }
            else
            {
                ilGenerator.Emit(OpCodes.Callvirt, MethodConstant.AspectActivator_Invoke.MakeGenericMethod(serviceMethod.ReturnType));
            }

            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
