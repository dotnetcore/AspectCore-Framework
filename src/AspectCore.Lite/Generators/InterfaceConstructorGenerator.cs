using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public sealed class InterfaceConstructorGenerator
    {
        private readonly ServiceProviderGenerator serviceProviderGenerator;
        private readonly ServiceInstanceGenerator serviceInstanceGenerator;
        private readonly TypeBuilder typeBuilder;
        private readonly Type serviceType;
        internal InterfaceConstructorGenerator(TypeBuilder typeBuilder, Type serviceType, ServiceProviderGenerator serviceProviderGenerator, ServiceInstanceGenerator serviceInstanceGenerator)
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
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Call, typeof(object).GetTypeInfo().DeclaredConstructors.FirstOrDefault());
            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.Emit(OpCodes.Stfld, serviceProviderGenerator.ServiceProviderBuilder);
            il.EmitLoadArg(0);
            il.EmitLoadArg(2);
            il.Emit(OpCodes.Stfld, serviceInstanceGenerator.ServiceInstanceBuilder);
            il.Emit(OpCodes.Ret);
        }
    }
}
