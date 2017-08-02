using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class CustomAttributeReflectorTests
    {
        [Fact]
        public void Invoke_Test()
        {
            var field = typeof(FieldFakes).GetTypeInfo().GetField("StaticFiled");
            var fieldReflector = field.GetReflector();
            var customAttributeReflectors = fieldReflector.CustomAttributeReflectors.ToArray();
            var attributeFakes = customAttributeReflectors[0].Invoke();
        }

        [Fact]
        public void Invoke_With_ConstructorArguments_Test()
        {
            var field = typeof(FieldFakes).GetTypeInfo().GetField("InstanceField");
            var fieldReflector = field.GetReflector();
            var customAttributeReflectors = fieldReflector.CustomAttributeReflectors.ToArray();
            var attributeFakes = (AttributeFakes1)customAttributeReflectors[0].Invoke();
            Assert.Equal(100, attributeFakes.Id);
        }

        [Fact]
        public void Invoke_With_NamedArguments_Test()
        {
            var field = typeof(FieldFakes<>).GetTypeInfo().GetField("StaticFiled");
            var fieldReflector = field.GetReflector();
            var customAttributeReflectors = fieldReflector.CustomAttributeReflectors.ToArray();
            var attributeFakes = (AttributeFakes2)customAttributeReflectors[0].Invoke();
            Assert.Equal(100, attributeFakes.Id);
            Assert.Null(attributeFakes.Obj);
            Assert.Equal("Lemon", attributeFakes.Name);
            Assert.Equal(typeof(FieldFakes<>), attributeFakes.Type);
        }

        [Fact]
        public void Invoke_With_Array_Test()
        {
            var field = typeof(FieldFakes<>).GetTypeInfo().GetField("InstanceField");
            var fieldReflector = field.GetReflector();
            var attributeFakes = fieldReflector.GetCustomAttribute<AttributeFakes3>();
            var types = new Type[] { typeof(int), typeof(long) };
            var ids = new int[] { 1, 2, 3 };
            Assert.Equal(types.Length, attributeFakes.Types.Length);
            for (var i = 0; i < types.Length; i++)
            {
                Assert.Equal(types[i], attributeFakes.Types[i]);
            }
            Assert.Equal(ids.Length, attributeFakes.Ids.Length);
            for (var i = 0; i < types.Length; i++)
            {
                Assert.Equal(ids[i], attributeFakes.Ids[i]);
            }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AttributeFakes : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AttributeFakes1 : AttributeFakes
    {
        public int Id { get; }
        public AttributeFakes1(int id) { Id = id; }
    }
    public class AttributeFakes2 : Attribute
    {
        public int Id { get; } = 100;

        public object Obj { get; set; } = "lemon";

        public string Name { get; set; } = "lemon";

        public Type Type { get; set; }

        public AttributeFakes2(Type type) { Type = type; }
    }
    public class AttributeFakes3 : Attribute
    {
        public int[] Ids { get; set; }

        public Type[] Types { get; }

        public AttributeFakes3(params Type[] types)
        {
            Types = types;
        }
    }
}