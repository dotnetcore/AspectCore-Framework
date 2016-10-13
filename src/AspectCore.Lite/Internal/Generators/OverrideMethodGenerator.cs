using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public class OverrideMethodGenerator : InterfaceMethodGenerator
    {
        public OverrideMethodGenerator(TypeBuilder typeBuilder, MethodInfo method, FieldGenerator serviceInstanceGenerator, FieldGenerator serviceProviderGenerator)
            : base(typeBuilder, method, serviceInstanceGenerator, serviceProviderGenerator)
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
