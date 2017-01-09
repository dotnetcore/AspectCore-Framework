using AspectCore.Lite.Abstractions.Common;
using System.Reflection;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class MethodAccessorTest
    {
        public object TestMethod()
        {
            return "test";
        }

        [Fact]
        public void MethodAccessorInvoker_Test()
        {
            var methodInfo = typeof(MethodAccessorTest).GetTypeInfo().GetMethod("TestMethod");
            var methodAccessor = new MethodAccessor(methodInfo);
            var methodInvoker = methodAccessor.CreateMethodInvoker();
            Assert.Equal(methodInvoker.Invoke(this, EmptyArray<object>.Value), TestMethod());
        }
    }
}
