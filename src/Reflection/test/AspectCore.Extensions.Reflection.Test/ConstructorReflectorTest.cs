using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class ConstructorReflectorTest
    {
        [Fact]
        public void NonparametricTest()
        {
            var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[0]);
            var reflector = constructor.AsReflector();
            var fakes = (ConstructorFakes)reflector.Invoke();
            Assert.IsType(typeof(ConstructorFakes), fakes);
            Assert.Equal("Nonparametric constructor", fakes.Name);
        }

        [Fact]
        public void ParametricTest()
        {
            var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
            var reflector = constructor.AsReflector();
            var fakes = (ConstructorFakes)reflector.Invoke("test");
            Assert.IsType(typeof(ConstructorFakes), fakes);
            Assert.Equal("Parametric constructor. param : test", fakes.Name);
        }

        [Fact]
        public void ParametricByRefTest()
        {
            var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[] { typeof(string).MakeByRefType(), typeof(ConstructorFakes).MakeByRefType() });
            var reflector = constructor.AsReflector();
            var args = new object[] { "test", new ConstructorFakes("test") };
            var fakes = (ConstructorFakes)reflector.Invoke(args);
            Assert.IsType(typeof(ConstructorFakes), fakes);
            Assert.Equal("Parametric constructor with ref param.", fakes.Name);
            Assert.Equal(fakes.Name, args[0]);
            Assert.Equal("Nonparametric constructor", ((ConstructorFakes)args[1]).Name);
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

        public ConstructorFakes(ref string name, ref ConstructorFakes fakes)
        {
            name = Name = "Parametric constructor with ref param.";
            fakes = new ConstructorFakes();
        }
    }
}
