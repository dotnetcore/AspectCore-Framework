using System;
using System.Collections.Generic;
using AspectCore.DynamicProxy;
using System.Text;
using Xunit;
using System.Threading.Tasks;

namespace AspectCore.Tests.DynamicProxy
{
    public class AsyncAspectTests : DynamicProxyTestBase
    {
        [Fact]
        public void DynAsync_Test()
        {
            var proxy = ProxyGenerator.CreateClassProxy<FakeAsyncClass>();
            var result = proxy.DynAsync(100);
            Assert.IsAssignableFrom<Task<int>>(result);
        }

        [Fact]
        public void Async_Test()
        {
            var proxy = ProxyGenerator.CreateClassProxy<FakeAsyncClass>();
            var result = proxy.Async(100);
            Assert.IsAssignableFrom<Task<int>>(result);
        }
    }
}
