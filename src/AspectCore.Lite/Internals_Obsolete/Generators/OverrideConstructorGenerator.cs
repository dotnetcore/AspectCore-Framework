using AspectCore.Lite.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Lite.Abstractions;

namespace AspectCore.Lite.Internals.Generators
{
    internal class OverrideConstructorGenerator
    {
        private readonly FieldGenerator serviceProviderGenerator;
        private readonly FieldGenerator serviceInstanceGenerator;
        private readonly TypeBuilder typeBuilder;
        private readonly Type serviceType;

        private static readonly MethodInfo GetServiceMethod =
            GeneratorHelper.GetMethodInfo<Func<IOriginalServiceProvider, Type, object>>(
                (serviceProvider, serviceType) => serviceProvider.GetService(serviceType));

        internal OverrideConstructorGenerator(TypeBuilder typeBuilder, Type serviceType,
            FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            this.serviceProviderGenerator = serviceProviderGenerator;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
            this.typeBuilder = typeBuilder;
            this.serviceType = serviceType;
        }

        public void GenerateConstructor()
        {
            var serviceTypeParameters = new Type[] {typeof(IServiceProvider), typeof(IOriginalServiceProvider)};
            var contructors =
                serviceType.GetTypeInfo()
                    .DeclaredConstructors.Where(
                        c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly))
                    .ToArray();

            if (contructors.Length == 0)
            {
                var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                    serviceTypeParameters);
                var il = constructor.GetILGenerator();
                il.EmitThis();
                il.Emit(OpCodes.Call, typeof(object).GetTypeInfo().DeclaredConstructors.FirstOrDefault());
                il.EmitThis();
                il.EmitLoadArg(1);
                il.Emit(OpCodes.Stfld, serviceProviderGenerator.FieldBuilder);
                GenerateServiceInstance(il, 2, serviceInstanceGenerator);
                il.Emit(OpCodes.Ret);
                return;
            }

            foreach (var ctor in contructors)
            {
                var parameters =
                    ctor.GetParameters().Select(p => p.ParameterType).Concat(serviceTypeParameters).ToArray();

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
                GenerateServiceInstance(il, parameters.Length, serviceInstanceGenerator);
                il.Emit(OpCodes.Ret);
            }
        }

        private static void GenerateServiceInstance(ILGenerator il, int argIndex,FieldGenerator serviceInstance)
        {
            il.EmitThis();
            il.EmitLoadArg(argIndex);
            il.EmitTypeof(serviceInstance.FieldType);
            il.Emit(OpCodes.Callvirt, GetServiceMethod);
            il.EmitConvertToType(typeof(object), serviceInstance.FieldType, false);
            il.Emit(OpCodes.Stfld, serviceInstance.FieldBuilder);
        }
    }
}
