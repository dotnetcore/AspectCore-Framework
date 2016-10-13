using AspectCore.Lite.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AspectCore.Lite.Extensions;

namespace AspectCore.Lite.Generators
{
    public class InterceptedMethodBodyGenerator : MethodBodyGenerator
    {
        private readonly MethodInfo GetAspectExecutorMethod = GeneratorUtilities.GetMethodInfo<Func<IServiceProvider, IAspectExecutor>>(serviceProvider => serviceProvider.GetRequiredService<IAspectExecutor>());
        private readonly MethodInfo AspectExecuteSynchronouslyMethod = typeof(IAspectExecutor).GetTypeInfo().GetMethod("ExecuteSynchronously");
        private readonly MethodInfo AspectExecuteAsyncMethod = typeof(IAspectExecutor).GetTypeInfo().GetMethod("ExecuteAsync");

        protected FieldGenerator serviceProviderGenerator;

        internal InterceptedMethodBodyGenerator(InterfaceMethodGenerator methodGenerator , FieldGenerator serviceInstanceGenerator, FieldGenerator serviceProviderGenerator) 
            : base(methodGenerator , serviceInstanceGenerator)
        {
            this.serviceProviderGenerator = serviceProviderGenerator;
        }

        public override void GenerateMethodBody()
        {
            var il = methodGenerator.MethodBuilder.GetILGenerator();
            var parameters = methodGenerator.TargetMethod.GetParameters().Select(x => x.ParameterType).ToArray();
            var method = methodGenerator.TargetMethod;    

            il.EmitThis();
            il.Emit(OpCodes.Ldfld, serviceProviderGenerator.FieldBuilder);
            il.Emit(OpCodes.Call, GetAspectExecutorMethod);
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, serviceInstanceGenerator.FieldBuilder);
            il.EmitThis();
            il.EmitTypeof(method.DeclaringType);
            il.Emit(OpCodes.Ldstr, method.Name);
            il.EmitLoadInt(parameters.Length);
            il.Emit(OpCodes.Newarr,typeof(object));

            for (int i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.EmitLoadInt(i);
                il.EmitLoadArg(i + 1); 
                il.EmitConvertToType(parameters[i], typeof(object), false);
                il.Emit(OpCodes.Stelem_Ref);
            }

            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Callvirt, AspectExecuteSynchronouslyMethod.MakeGenericMethod(typeof(object)));
            }
            else if(method.IsReturnTask())
            {
                il.Emit(OpCodes.Callvirt, AspectExecuteAsyncMethod.MakeGenericMethod(method.ReturnType));
            }
            else
            {
                il.Emit(OpCodes.Callvirt, AspectExecuteSynchronouslyMethod.MakeGenericMethod(method.ReturnType));
            }

            il.Emit(OpCodes.Ret);
        }
    }
}
