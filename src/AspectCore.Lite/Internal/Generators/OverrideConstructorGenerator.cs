using AspectCore.Lite.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internal.Generators
{
    internal class OverrideConstructorGenerator
    {
        private readonly FieldGenerator serviceProviderGenerator;
        private readonly FieldGenerator serviceInstanceGenerator;
        private readonly TypeBuilder typeBuilder;
        private readonly Type serviceType;

        internal OverrideConstructorGenerator(TypeBuilder typeBuilder, Type serviceType, FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            this.serviceProviderGenerator = serviceProviderGenerator;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
            this.typeBuilder = typeBuilder;
            this.serviceType = serviceType;
        }

        public void GenerateConstructor()
        {
            var serviceTypeParameters = new Type[] { typeof(IServiceProvider), serviceType };
            var contructors = serviceType.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic && !c.IsStatic).ToArray();

            if (contructors.Length == 0)
            {
                var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, serviceTypeParameters);
                var il = constructor.GetILGenerator();
                il.EmitThis();
                il.Emit(OpCodes.Call, typeof(object).GetTypeInfo().DeclaredConstructors.FirstOrDefault());
                il.EmitThis();
                il.EmitLoadArg(1);
                il.Emit(OpCodes.Stfld, serviceProviderGenerator.FieldBuilder);
                il.EmitThis();
                il.EmitLoadArg(2);
                il.Emit(OpCodes.Stfld, serviceInstanceGenerator.FieldBuilder);
                il.Emit(OpCodes.Ret);
                return;
            }

            foreach (var ctor in contructors)
            {
                var parameters = ctor.GetParameters().Select(p => p.ParameterType).Concat(serviceTypeParameters).ToArray();

                var constructor = typeBuilder.DefineConstructor(ctor.Attributes, ctor.CallingConvention, parameters);
                var il = constructor.GetILGenerator();
                il.EmitThis();

                for (var i = 1; i <= parameters.Length - 2; i++)
                {
                    il.EmitLoadArg(i);
                }

                il.Emit(OpCodes.Call, ctor);
                il.EmitThis();
                il.EmitLoadArg(parameters.Length - 1);
                il.Emit(OpCodes.Stfld, serviceProviderGenerator.FieldBuilder);
                il.EmitThis();
                il.EmitLoadArg(parameters.Length);
                il.Emit(OpCodes.Stfld, serviceInstanceGenerator.FieldBuilder);
                il.Emit(OpCodes.Ret);
            }
        }
    }
}
