using AspectCore.Lite.Abstractions;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internals.Generators
{
    internal class OverrideMethodGenerator : InterfaceMethodGenerator
    {
        public OverrideMethodGenerator(TypeBuilder typeBuilder, MethodInfo method, FieldGenerator serviceInstanceGenerator, FieldGenerator serviceProviderGenerator, IPointcut pointcut)
            : base(typeBuilder, method, serviceInstanceGenerator, serviceProviderGenerator, pointcut)
        {
        }

        public override void GenerateMethod()
        {
            var parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();
            builder = typeBuilder.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                method.ReturnType, parameters);

            GenerateGenericParameter();

            var methodBody = GetMethodBodyGenerator();
            methodBody.GenerateMethodBody();
        }
    }
}
