using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public class InterfaceMethodGenerator
    {
        private readonly TypeBuilder typeBuilder;
        private readonly ServiceInstanceGenerator serviceInstanceGenerator;
        private readonly MethodInfo method;
        private MethodBuilder builder;
        public MethodBuilder MethodBuilder => builder;

        internal InterfaceMethodGenerator(TypeBuilder typeBuilder, MethodInfo method, ServiceInstanceGenerator serviceInstanceGenerator)
        {
            this.typeBuilder = typeBuilder;
            this.method = method;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
        }

        public virtual void GenerateMethod()
        {
            var parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();
            builder = typeBuilder.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot,
                method.ReturnType, parameters);

            var il = builder.GetILGenerator();
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Ldfld, serviceInstanceGenerator.ServiceInstanceBuilder);
            for (int i = 1; i <= parameters.Length; i++)
            {
                il.EmitLoadArg(i);
            }
            il.Emit(OpCodes.Callvirt, method);
            il.Emit(OpCodes.Ret);
        }
    }
}
