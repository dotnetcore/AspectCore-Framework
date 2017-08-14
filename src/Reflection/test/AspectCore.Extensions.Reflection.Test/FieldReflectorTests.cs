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

        [Fact]
        public void Instance_StructField_Get_Test()
        {
            var fakes = new FieldFakes();
            fakes.StructField = 100;
            var field = typeof(FieldFakes).GetTypeInfo().GetField("StructField");
            var fieldReflector = field.GetReflector();
            var fieldValue = fieldReflector.GetValue(fakes);
            Assert.Equal(100, fieldValue);
        }

        [Fact]
        public void StructInstance_Field_Get_Test()
        {
            var fakes = new StructFieldFakes();
            fakes.ClassField = "Lemon";
            var field = typeof(StructFieldFakes).GetTypeInfo().GetField("ClassField");
            var fieldReflector = field.GetReflector();
            var fieldValue = fieldReflector.GetValue(fakes);
            Assert.Equal("Lemon", fieldValue);
        }


        [Fact]
        public void StructInstance_StructField_Get_Test()
        {
            var fakes = new StructFieldFakes();
            fakes.StructField = 100;
            var field = typeof(StructFieldFakes).GetTypeInfo().GetField("StructField");
            var fieldReflector = field.GetReflector();
            var fieldValue = fieldReflector.GetValue(fakes);
            Assert.Equal(100, fieldValue);
        }
    }

    public class FieldFakes
    {
        [AttributeFakes]
        public static string StaticFiled;

        [AttributeFakes1(100)]
        public string InstanceField;

        public int StructField;
    }

    public class FieldFakes<T>
    {
        [AttributeFakes2(typeof(FieldFakes<>), Name = "Lemon", Obj = null)]
        public static T StaticFiled;

        [AttributeFakes3(typeof(int), typeof(long), Ids = new int[] { 1, 2, 3 })]
        public T InstanceField;
    }

    public struct StructFieldFakes
    {
        public string ClassField;

        public int StructField;
    }
}