using AspectCore.Lite.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public class GenericMethodGenerator
    {
        private readonly InterfaceMethodGenerator methodGenreator;

        internal GenericMethodGenerator(InterfaceMethodGenerator interfaceMethodGenerator)
        {
            methodGenreator = interfaceMethodGenerator;
        }

        public void GenerateGenericParameter()
        {
            var genericArguments = methodGenreator.TargetMethod.GetGenericArguments().Select(t => t.GetTypeInfo());
            var genericArgumentsBuilders = methodGenreator.MethodBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            genericArguments.ForEach((arg, index) =>
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(arg.GenericParameterAttributes);
                arg.GetGenericParameterConstraints().Select(t => t.GetTypeInfo()).ForEach(constraint =>
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                });
            });
        }
    }
}
