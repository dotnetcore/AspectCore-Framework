using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class ReflectorExtensionsTests
    {
        #region GetReflector - Type

        [Fact]
        public void GetReflector_Type_Null_Throws()
        {
            Type type = null;
            Assert.Throws<ArgumentNullException>(() => type.GetReflector());
        }

        [Fact]
        public void GetReflector_Type_Works()
        {
            var reflector = typeof(ReflectorTestClass).GetReflector();
            Assert.NotNull(reflector);
        }

        #endregion

        #region GetReflector - TypeInfo

        [Fact]
        public void GetReflector_TypeInfo_Null_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.GetReflector());
        }

        [Fact]
        public void GetReflector_TypeInfo_Works()
        {
            var reflector = typeof(ReflectorTestClass).GetTypeInfo().GetReflector();
            Assert.NotNull(reflector);
        }

        #endregion

        #region GetReflector - ConstructorInfo

        [Fact]
        public void GetReflector_ConstructorInfo_Null_Throws()
        {
            ConstructorInfo constructor = null;
            Assert.Throws<ArgumentNullException>(() => constructor.GetReflector());
        }

        [Fact]
        public void GetReflector_ConstructorInfo_Works()
        {
            var constructor = typeof(ReflectorTestClass).GetConstructor(Type.EmptyTypes);
            var reflector = constructor.GetReflector();
            Assert.NotNull(reflector);
        }

        #endregion

        #region GetReflector - FieldInfo

        [Fact]
        public void GetReflector_FieldInfo_Null_Throws()
        {
            FieldInfo field = null;
            Assert.Throws<ArgumentNullException>(() => field.GetReflector());
        }

        [Fact]
        public void GetReflector_FieldInfo_Works()
        {
            var field = typeof(ReflectorTestClass).GetField("PublicField");
            var reflector = field.GetReflector();
            Assert.NotNull(reflector);
        }

        #endregion

        #region GetReflector - MethodInfo

        [Fact]
        public void GetReflector_MethodInfo_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.GetReflector());
        }

        [Fact]
        public void GetReflector_MethodInfo_Works()
        {
            var method = typeof(ReflectorTestClass).GetMethod("PublicMethod");
            var reflector = method.GetReflector();
            Assert.NotNull(reflector);
        }

        [Fact]
        public void GetReflector_MethodInfo_With_CallOptions_Works()
        {
            var method = typeof(ReflectorTestClass).GetMethod("PublicMethod");
            var reflector = method.GetReflector(CallOptions.Callvirt);
            Assert.NotNull(reflector);
        }

        #endregion

        #region GetReflector - PropertyInfo

        [Fact]
        public void GetReflector_PropertyInfo_Null_Throws()
        {
            PropertyInfo property = null;
            Assert.Throws<ArgumentNullException>(() => property.GetReflector());
        }

        [Fact]
        public void GetReflector_PropertyInfo_Works()
        {
            var property = typeof(ReflectorTestClass).GetProperty("PublicProperty");
            var reflector = property.GetReflector();
            Assert.NotNull(reflector);
        }

        [Fact]
        public void GetReflector_PropertyInfo_With_CallOptions_Works()
        {
            var property = typeof(ReflectorTestClass).GetProperty("PublicProperty");
            var reflector = property.GetReflector(CallOptions.Callvirt);
            Assert.NotNull(reflector);
        }

        #endregion

        #region GetReflector - ParameterInfo

        [Fact]
        public void GetReflector_ParameterInfo_Null_Throws()
        {
            ParameterInfo parameter = null;
            Assert.Throws<ArgumentNullException>(() => parameter.GetReflector());
        }

        [Fact]
        public void GetReflector_ParameterInfo_Works()
        {
            var method = typeof(ReflectorTestClass).GetMethod("MethodWithParam");
            var parameter = method.GetParameters()[0];
            var reflector = parameter.GetReflector();
            Assert.NotNull(reflector);
        }

        #endregion

        #region GetFieldInfo / GetMethodInfo / GetConstructorInfo / GetPropertyInfo

        [Fact]
        public void GetFieldInfo_Works()
        {
            var field = typeof(ReflectorTestClass).GetField("PublicField");
            var reflector = field.GetReflector();
            var fieldInfo = reflector.GetFieldInfo();
            Assert.Equal(field, fieldInfo);
        }

        [Fact]
        public void GetFieldInfo_Null_Returns_Null()
        {
            FieldReflector reflector = null;
            Assert.Null(reflector.GetFieldInfo());
        }

        [Fact]
        public void GetMethodInfo_Works()
        {
            var method = typeof(ReflectorTestClass).GetMethod("PublicMethod");
            var reflector = method.GetReflector();
            var methodInfo = reflector.GetMethodInfo();
            Assert.Equal(method, methodInfo);
        }

        [Fact]
        public void GetMethodInfo_Null_Returns_Null()
        {
            MethodReflector reflector = null;
            Assert.Null(reflector.GetMethodInfo());
        }

        [Fact]
        public void GetConstructorInfo_Works()
        {
            var constructor = typeof(ReflectorTestClass).GetConstructor(Type.EmptyTypes);
            var reflector = constructor.GetReflector();
            var constructorInfo = reflector.GetConstructorInfo();
            Assert.Equal(constructor, constructorInfo);
        }

        [Fact]
        public void GetConstructorInfo_Null_Returns_Null()
        {
            ConstructorReflector reflector = null;
            Assert.Null(reflector.GetConstructorInfo());
        }

        [Fact]
        public void GetPropertyInfo_Works()
        {
            var property = typeof(ReflectorTestClass).GetProperty("PublicProperty");
            var reflector = property.GetReflector();
            var propertyInfo = reflector.GetPropertyInfo();
            Assert.Equal(property, propertyInfo);
        }

        [Fact]
        public void GetPropertyInfo_Null_Returns_Null()
        {
            PropertyReflector reflector = null;
            Assert.Null(reflector.GetPropertyInfo());
        }

        #endregion
    }

    public class ReflectorTestClass
    {
        public int PublicField;
        public string PublicProperty { get; set; }
        public void PublicMethod() { }
        public void MethodWithParam(int param) { }
    }
}
