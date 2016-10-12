using AspectCore.Lite.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public class InterceptedMethodBodyGenerator : MethodBodyGenerator
    {
        //private readonly MethodInfo AspectExecutorMethod = typeof(ServiceProviderServiceExtensions).GetTypeInfo().DeclaredMethods.
        //            Single(m => m.Name == GeneratorConstants.GetRequiredService && m.IsGenericMethod).MakeGenericMethod(typeof(IAspectExecutor));

        private readonly MethodInfo AspectExecutorMethod = GeneratorUtilities.GetMethodInfo<Func<IServiceProvider, IAspectExecutor>>(serviceProvider => serviceProvider.GetRequiredService<IAspectExecutor>());

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
            il.Emit(OpCodes.Call, AspectExecutorMethod);
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

            il.Emit(OpCodes.Callvirt, typeof(IAspectExecutor).GetTypeInfo().GetMethod("ExecuteSynchronously"));

            if (method.ReturnType != typeof(void))
            {
                il.EmitConvertToType(method.ReturnType, typeof(object), true);
            }

            il.Emit(OpCodes.Ret);
        }
    }
}
