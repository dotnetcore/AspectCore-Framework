using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    public class PropertyGenerator
    {
        protected readonly TypeBuilder typeBuilder;
        protected readonly PropertyInfo propertyInfo;
        protected readonly FieldGenerator serviceInstanceGenerator;
        protected readonly FieldGenerator serviceProviderGenerator;

        protected internal PropertyGenerator(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldGenerator serviceInstanceGenerator, FieldGenerator serviceProviderGenerator)
        {
            this.typeBuilder = typeBuilder;
            this.propertyInfo = propertyInfo;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
            this.serviceProviderGenerator = serviceProviderGenerator;
        }

        public virtual void GenerateProperty()
        {
            var property = typeBuilder.DefineProperty($"{propertyInfo.DeclaringType.FullName}.{propertyInfo.Name}", PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

            if (propertyInfo.CanRead)
            {
                var methodGenerator = new InterfaceMethodGenerator(typeBuilder, propertyInfo.GetMethod, serviceInstanceGenerator, serviceProviderGenerator);
                methodGenerator.GenerateMethod();
                property.SetGetMethod(methodGenerator.MethodBuilder);
            }

            if (propertyInfo.CanWrite)
            {
                var methodGenerator = new InterfaceMethodGenerator(typeBuilder, propertyInfo.SetMethod, serviceInstanceGenerator, serviceProviderGenerator);
                methodGenerator.GenerateMethod();
                property.SetSetMethod(methodGenerator.MethodBuilder);
            }
        }
    }
}
