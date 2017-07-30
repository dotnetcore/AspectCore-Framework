using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class FieldReflectorTests
    {
        [Fact]
        public void InstanceField_Get_Test()
        {
            var fakes = new FieldFakes();
            fakes.InstanceField = "InstanceField";
            var field = typeof(FieldFakes).GetTypeInfo().GetField("InstanceField");
            var fieldReflector = field.GetReflector();
            var fieldValue = fieldReflector.GetValue(fakes);
            Assert.Equal("InstanceField", fieldValue);
        }

        [Fact]
        public void InstanceField_Set_Test()
        {
            var fakes = new FieldFakes();
            var field = typeof(FieldFakes).GetTypeInfo().GetField("InstanceField");
            var fieldReflector = field.GetReflector();
            fieldReflector.SetValue(fakes, "InstanceField");
            Assert.Equal("InstanceField", fakes.InstanceField);
        }

        [Fact]
        public void StaticField_Get_Test()
        {
            FieldFakes.StaticFiled = "StaticFiled";
            var field = typeof(FieldFakes).GetTypeInfo().GetField("StaticFiled");
            var fieldReflector = field.GetReflector();
            var fieldValue = fieldReflector.GetStaticValue();
            Assert.Equal("StaticFiled", fieldValue);
        }

        [Fact]
        public void StaticField_Set_Test()
        {
            var field = typeof(FieldFakes).GetTypeInfo().GetField("StaticFiled");
            var fieldReflector = field.GetReflector();
            fieldReflector.SetStaticValue("StaticFiled");
            Assert.Equal("StaticFiled", FieldFakes.StaticFiled);
        }

        [Fact]
        public void StaticField_OfT_Test()
        {
            var field = typeof(FieldFakes<string>).GetTypeInfo().GetField("StaticFiled");
            var fieldReflector = field.GetReflector();
            fieldReflector.SetStaticValue("StaticFiled");
            Assert.Equal("StaticFiled", fieldReflector.GetStaticValue());
        }
    }

    public class FieldFakes
    {
        public static string StaticFiled;

        public string InstanceField;
    }

    public class FieldFakes<T>
    {
        public static T StaticFiled;

        public T InstanceField;
    }
}