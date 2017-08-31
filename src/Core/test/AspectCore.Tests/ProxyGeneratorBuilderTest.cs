using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Core.DynamicProxy;
using AspectCore.Core.Configuration;
using Xunit;

namespace AspectCore.Tests
{
    public class ProxyGeneratorBuilderTest
    {
        [Fact]
        public void CreateInterfaceProxy()
        {
            Action<AspectCoreOptions> options = option =>
             {
                 option.InterceptorFactories.AddDelegate(async (ctx, next) =>
                 {
                     await next(ctx);
                     ctx.ReturnValue = "lemon";
                 }, 
                 Predicates.ForMethod("get_*"));
             };

            var proxyGenerator = new ProxyGeneratorBuilder().Configure(options).Build();

            var serviceProxy = (IService)proxyGenerator.CreateInterfaceProxy(typeof(IService));

            Assert.Equal("lemon", serviceProxy.Name);
        }
    }
}
