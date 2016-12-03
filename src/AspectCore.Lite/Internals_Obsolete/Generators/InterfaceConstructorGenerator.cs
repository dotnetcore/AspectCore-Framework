using AspectCore.Lite.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internals.Generators
{
    internal sealed class InterfaceConstructorGenerator
    {
        private readonly FieldGenerator serviceProviderGenerator;
        private readonly FieldGenerator serviceInstanceGenerator;
        private readonly TypeBuilder typeBuilder;
        private readonly Type serviceType;
        internal InterfaceConstructorGenerator(TypeBuilder typeBuilder, Type serviceType, FieldGenerator serviceProviderGenerator, FieldGenerator serviceInstanceGenerator)
        {
            this.serviceProviderGenerator = serviceProviderGenerator;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
            this.typeBuilder = typeBuilder;
            this.serviceType = serviceType;
        }

        public void GenerateConstructor()
        {
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IServiceProvider), serviceType });
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
        }
    }
}
