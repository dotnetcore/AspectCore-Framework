using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class OpenGenericCoverageTests
    {
        [Fact]
        public void OpenGeneric_FieldReflector_GetValue_Throws()
        {
            var field = typeof(FieldFakes<>).GetTypeInfo().GetField("InstanceField");
            var reflector = field.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.GetValue(new object()));
        }

        [Fact]
        public void OpenGeneric_FieldReflector_SetValue_Throws()
        {
            var field = typeof(FieldFakes<>).GetTypeInfo().GetField("InstanceField");
            var reflector = field.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.SetValue(new object(), "test"));
        }

        [Fact]
        public void OpenGeneric_FieldReflector_GetStaticValue_Throws()
        {
            var field = typeof(FieldFakes<>).GetTypeInfo().GetField("StaticFiled");
            var reflector = field.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.GetStaticValue());
        }

        [Fact]
        public void OpenGeneric_FieldReflector_SetStaticValue_Throws()
        {
            var field = typeof(FieldFakes<>).GetTypeInfo().GetField("StaticFiled");
            var reflector = field.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.SetStaticValue("test"));
        }

        [Fact]
        public void OpenGeneric_PropertyReflector_GetValue_Throws()
        {
            var property = typeof(PropertyFakes<>).GetTypeInfo().GetProperty("InstanceProperty");
            var reflector = property.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.GetValue(new object()));
        }

        [Fact]
        public void OpenGeneric_PropertyReflector_SetValue_Throws()
        {
            var property = typeof(PropertyFakes<>).GetTypeInfo().GetProperty("InstanceProperty");
            var reflector = property.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.SetValue(new object(), "test"));
        }

        [Fact]
        public void OpenGeneric_PropertyReflector_GetStaticValue_Throws()
        {
            var property = typeof(PropertyFakes<>).GetTypeInfo().GetProperty("StaticProperty");
            var reflector = property.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.GetStaticValue());
        }

        [Fact]
        public void OpenGeneric_PropertyReflector_SetStaticValue_Throws()
        {
            var property = typeof(PropertyFakes<>).GetTypeInfo().GetProperty("StaticProperty");
            var reflector = property.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.SetStaticValue("test"));
        }

        [Fact]
        public void OpenGeneric_MethodReflector_Invoke_Throws()
        {
            var method = typeof(MethodFakes<>).GetMethod("GetString");
            var reflector = method.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.Invoke(new object(), "test"));
        }

        [Fact]
        public void OpenGeneric_MethodReflector_StaticInvoke_Throws()
        {
            var method = typeof(MethodFakes<>).GetMethod("GetString");
            var reflector = method.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.StaticInvoke("test"));
        }

        [Fact]
        public void OpenGeneric_ConstructorReflector_Invoke_Throws()
        {
            var constructor = typeof(ConstructorFakes<>).GetTypeInfo().DeclaredConstructors.First();
            var reflector = constructor.GetReflector();
            Assert.Throws<InvalidOperationException>(() => reflector.Invoke());
        }
    }

    public class ParameterReflectorCoverageTests
    {
        [Fact]
        public void ParameterReflector_Name_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithParams");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.Equal("a", reflector.Name);
        }

        [Fact]
        public void ParameterReflector_Position_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithParams");
            var parameters = method.GetParameters();
            var reflector0 = parameters[0].GetReflector();
            var reflector1 = parameters[1].GetReflector();
            Assert.Equal(0, reflector0.Position);
            Assert.Equal(1, reflector1.Position);
        }

        [Fact]
        public void ParameterReflector_ParameterType_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithParams");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.Equal(typeof(int), reflector.ParameterType);
        }

        [Fact]
        public void ParameterReflector_HasDefaultValue_True_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithDefault");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.True(reflector.HasDeflautValue);
        }

        [Fact]
        public void ParameterReflector_HasDefaultValue_False_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithParams");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.False(reflector.HasDeflautValue);
        }

        [Fact]
        public void ParameterReflector_DefaultValue_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithDefault");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.Equal(42, reflector.DefalutValue);
        }

        [Fact]
        public void ParameterReflector_GetParameterInfo_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithParams");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.Same(param, reflector.GetParameterInfo());
        }

        [Fact]
        public void ParameterReflector_ToString_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithParams");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            var str = reflector.ToString();
            Assert.Contains("Parameter", str);
            Assert.Contains("ParameterType", str);
        }

        [Fact]
        public void ParameterReflector_CustomAttributeReflectors_Works()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithAttribute");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.NotNull(reflector.CustomAttributeReflectors);
            Assert.Single(reflector.CustomAttributeReflectors);
        }
    }

    public class ParameterInfoExtensionsCoverageTests
    {
        [Fact]
        public void DefaultValueSafely_StringDefault_Returns_Value()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithStringDefault");
            var param = method.GetParameters()[0];
            Assert.Equal("hello", param.DefaultValueSafely());
        }

        [Fact]
        public void DefaultValueSafely_NoDefault_Returns_Value()
        {
            var method = typeof(ParamFakes2).GetMethod("MethodWithParams");
            var param = method.GetParameters()[0];
            var result = param.DefaultValueSafely();
            // Should not throw
            Assert.NotNull(result);
        }
    }

    public class ParamFakes2
    {
        public void MethodWithParams(int a, string b) { }
        public void MethodWithDefault(int a = 42) { }
        public void MethodWithStringDefault(string a = "hello") { }
        public void MethodWithAttribute([ParamTest] int a) { }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParamTestAttribute : Attribute { }

    public class ConstructorFakes<T>
    {
        public ConstructorFakes() { }
        public ConstructorFakes(T value) { }
    }
}
