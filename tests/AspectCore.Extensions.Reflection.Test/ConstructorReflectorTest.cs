using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class ConstructorReflectorTests
    {
        [Fact]
        public void Nonparametric_Test()
        {
            var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[0]);
            var reflector = constructor.GetReflector();
            var fakes = (ConstructorFakes)reflector.Invoke();
            Assert.IsType<ConstructorFakes>(fakes);
            Assert.Equal("Nonparametric constructor", fakes.Name);
        }

        [Fact]
        public void Parametric_Test()
        {
            var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
            var reflector = constructor.GetReflector();
            var fakes = (ConstructorFakes)reflector.Invoke("test");
            Assert.IsType<ConstructorFakes>(fakes);
            Assert.Equal("Parametric constructor. param : test", fakes.Name);
        }

        [Fact]
        public void Parametric_ByRef_Test()
        {
            var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[] { typeof(string).MakeByRefType(), typeof(ConstructorFakes).MakeByRefType(), typeof(string) });
            var reflector = constructor.GetReflector();
            var args = new object[] { "test", new ConstructorFakes("test"), "t" };
            var fakes = (ConstructorFakes)reflector.Invoke(args);
            Assert.IsType<ConstructorFakes>(fakes);
            Assert.Equal("Parametric constructor with ref param.", fakes.Name);
            Assert.Equal(fakes.Name, args[0]);
            Assert.Equal("Nonparametric constructor", ((ConstructorFakes)args[1]).Name);
            Assert.NotEqual(fakes.Name, args[2]);
            Assert.Equal("t", args[2]);
        }

        // [Fact]
        internal void Struct_Nonparametric_Test()
        {
            var constructor = typeof(StructFakes).GetTypeInfo().GetConstructor(new Type[0]);
            var reflector = constructor.GetReflector();
            var fakes = (StructFakes)reflector.Invoke();
            Assert.IsType<StructFakes>(fakes);
            Assert.Equal("Nonparametric constructor", fakes.Name);
        }

        [Fact]
        public void Struct_Parametric_Test()
        {
            var constructor = typeof(StructFakes).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
            var reflector = constructor.GetReflector();
            var fakes = (StructFakes)reflector.Invoke("test");
            Assert.IsType<StructFakes>(fakes);
            Assert.Equal("Parametric constructor. param : test", fakes.Name);
        }

        [Fact]
        public void Struct_Parametric_ByRef_Test()
        {
            var constructor = typeof(StructFakes).GetTypeInfo().GetConstructor(new Type[] { typeof(string).MakeByRefType(), typeof(StructFakes).MakeByRefType(), typeof(string) });
            var reflector = constructor.GetReflector();
            var args = new object[] { "test", new StructFakes("test"), "t" };
            var fakes = (StructFakes)reflector.Invoke(args);
            Assert.IsType<StructFakes>(fakes);
            Assert.Equal("Parametric constructor with ref param.", fakes.Name);
            Assert.Equal(fakes.Name, args[0]); 
            Assert.NotEqual(fakes.Name, args[2]);
            Assert.Equal("t", args[2]);
        }
    }

    public struct StructFakes
    {
        public string Name { get; set; }

        public StructFakes(string name)
        {
            Name = "Parametric constructor. param : " + name;
        }

        public StructFakes(ref string name, ref StructFakes fakes, string lastName)
        {
            lastName = name = Name = "Parametric constructor with ref param.";
            fakes = new StructFakes();
        }
    }

    public class ConstructorFakes
    {
        public string Name { get; set; }
        public ConstructorFakes()
        {
            Name = "Nonparametric constructor";
        }

        public ConstructorFakes(string name)
        {
            Name = "Parametric constructor. param : " + name;
        }

        public ConstructorFakes(ref string name, ref ConstructorFakes fakes, string lastName)
        {
            lastName = name = Name = "Parametric constructor with ref param.";
            fakes = new ConstructorFakes();
        }
    }
}
