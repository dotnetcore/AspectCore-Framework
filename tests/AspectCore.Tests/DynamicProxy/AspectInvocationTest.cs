using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Configuration;
using Xunit;
using AspectCore.DynamicProxy;

namespace AspectCore.Tests.DynamicProxy
{
    public class AspectInvocationTest : DynamicProxyTestBase
    {
        [Fact]
        public void Synchronization_Method()
        {
            var service = ProxyGenerator.CreateClassProxy<AspectInvocationService>();
            Assert.Equal("Get", service.Get());
        }

        [Fact]
        public async Task Async_Task_Method()
        {
            var service = ProxyGenerator.CreateClassProxy<AspectInvocationService>();
            var task = service.GetAsync();
            Assert.Equal("GetAsync", await task);
        }

        [Fact]
        public async Task Async_ValueTask_Method()
        {
            var service = ProxyGenerator.CreateClassProxy<AspectInvocationService>();
            var task = service.GetValueAsync();
            Assert.Equal("GetValueAsync", await task);
        }

        [Fact]
        public void ValueTuple_Method()
        {
            var service = ProxyGenerator.CreateClassProxy<AspectInvocationService>();
            var tuple = service.GetTuple();
            Assert.Equal("GetTuple-Item1", tuple.Item1);
            Assert.Equal("GetTuple-Item2", tuple.Item2);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => next(ctx));
        }
    }

    public class AspectInvocationService
    {
        public virtual string Get()
        {
            return "Get";
        }

        public virtual Task<string> GetAsync()
        {
            return Task.FromResult("GetAsync");
        }

        public virtual ValueTask<string> GetValueAsync()
        {
            return new ValueTask<string>("GetValueAsync");
        }

        public virtual (string,string) GetTuple()
        {
            return ("GetTuple-Item1", "GetTuple-Item2");
        }
    }
}