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
        private readonly static MethodInfo AspectExecutorMethod = typeof(ServiceProviderServiceExtensions).GetTypeInfo().
            GetMethod(GeneratorConstants.GetRequiredService, Type.EmptyTypes).MakeGenericMethod(typeof(IAspectExecutor));

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

            il.EmitThis();
            il.Emit(OpCodes.Ldfld, serviceProviderGenerator.FieldBuilder);
            il.Emit(OpCodes.Call, AspectExecutorMethod);
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, serviceInstanceGenerator.FieldBuilder);
            il.EmitThis();

        }
    }
}
