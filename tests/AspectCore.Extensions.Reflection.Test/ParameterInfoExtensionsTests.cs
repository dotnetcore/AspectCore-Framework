using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class ParameterInfoExtensionsTests
    {
        [Fact]
        public void HasDefaultValueByAttributes_With_Default_Returns_True()
        {
            var method = typeof(ParamFakes).GetMethod("WithDefault");
            var param = method.GetParameters()[0];
            Assert.True(param.HasDefaultValueByAttributes());
        }

        [Fact]
        public void HasDefaultValueByAttributes_Without_Default_Returns_False()
        {
            var method = typeof(ParamFakes).GetMethod("WithoutDefault");
            var param = method.GetParameters()[0];
            Assert.False(param.HasDefaultValueByAttributes());
        }

        [Fact]
        public void DefaultValueSafely_With_Default_Returns_Value()
        {
            var method = typeof(ParamFakes).GetMethod("WithDefault");
            var param = method.GetParameters()[0];
            Assert.Equal(42, param.DefaultValueSafely());
        }

        [Fact]
        public void DefaultValueSafely_Without_Default_Does_Not_Throw()
        {
            var method = typeof(ParamFakes).GetMethod("WithoutDefault");
            var param = method.GetParameters()[0];
            var result = param.DefaultValueSafely();
            // Should not throw; result depends on runtime behavior
            Assert.NotNull(result);
        }

        [Fact]
        public void DefaultValueSafely_String_Default_Returns_Value()
        {
            var method = typeof(ParamFakes).GetMethod("WithStringDefault");
            var param = method.GetParameters()[0];
            Assert.Equal("hello", param.DefaultValueSafely());
        }
    }

    public class ParamFakes
    {
        public void WithDefault(int a = 42) { }
        public void WithoutDefault(int a) { }
        public void WithStringDefault(string a = "hello") { }
    }
}
