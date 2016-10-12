using AspectCore.Lite.Internal;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    public class InterfaceMethodGenerator
    {
        protected readonly TypeBuilder typeBuilder;
        protected readonly FieldGenerator serviceInstanceGenerator;
        protected readonly FieldGenerator serviceProviderGenerator;
        protected readonly MethodInfo method;
        protected MethodBuilder builder;
        public MethodBuilder MethodBuilder => builder;
        public MethodInfo TargetMethod => method;

        internal InterfaceMethodGenerator(TypeBuilder typeBuilder, MethodInfo method, FieldGenerator serviceInstanceGenerator, FieldGenerator serviceProviderGenerator)
        {
            this.typeBuilder = typeBuilder;
            this.method = method;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
            this.serviceProviderGenerator = serviceProviderGenerator;
        }

        public virtual void GenerateMethod()
        {
            var parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();
            builder = typeBuilder.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot,
                method.ReturnType, parameters);

            var methodBody = GetMethodBodyGenerator();
            methodBody.GenerateMethodBody();

        }


        private MethodBodyGenerator GetMethodBodyGenerator()
        {
            var pointcut = PointcutUtilities.GetPointcut(method.DeclaringType.GetTypeInfo());
            if (pointcut.IsMatch(method))
            {
                return new InterceptedMethodBodyGenerator(this, serviceInstanceGenerator, serviceProviderGenerator);
            }
            else
            {
                return new MethodBodyGenerator(this, serviceInstanceGenerator);
            }
        }
    }
}
