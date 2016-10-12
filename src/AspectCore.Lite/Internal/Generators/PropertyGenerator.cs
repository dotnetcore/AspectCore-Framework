using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    public class PropertyGenerator
    {
        private readonly TypeBuilder typeBuilder;
        private readonly PropertyInfo propertyInfo;
        private readonly FieldGenerator serviceInstanceGenerator;
        private readonly FieldGenerator serviceProviderGenerator;

        protected internal PropertyGenerator(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldGenerator serviceInstanceGenerator, FieldGenerator serviceProviderGenerator)
        {
            this.typeBuilder = typeBuilder;
            this.propertyInfo = propertyInfo;
            this.serviceInstanceGenerator = serviceInstanceGenerator;
            this.serviceProviderGenerator = serviceProviderGenerator;
        }

        public void GenerateProperty()
        {
            var property = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

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
