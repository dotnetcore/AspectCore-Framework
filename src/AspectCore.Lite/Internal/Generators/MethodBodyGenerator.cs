using AspectCore.Lite.Extensions;
using System.Linq;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    public class MethodBodyGenerator
    {
        protected readonly InterfaceMethodGenerator methodGenerator;
        protected readonly FieldGenerator serviceInstanceGenerator;

        internal MethodBodyGenerator(InterfaceMethodGenerator methodGenerator, FieldGenerator serviceInstanceGenerator)
        {
            this.methodGenerator = methodGenerator;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
        }

        public virtual void GenerateMethodBody()
        {
            var il = methodGenerator.MethodBuilder.GetILGenerator();
            var parameters = methodGenerator.TargetMethod.GetParameters().Select(x => x.ParameterType).ToArray();

            il.EmitThis();
            il.Emit(OpCodes.Ldfld, serviceInstanceGenerator.FieldBuilder);
            for (int i = 1; i <= parameters.Length; i++)
            {
                il.EmitLoadArg(i);
            }
            il.Emit(OpCodes.Callvirt, methodGenerator.TargetMethod);
            il.Emit(OpCodes.Ret);
        }
    }
}
