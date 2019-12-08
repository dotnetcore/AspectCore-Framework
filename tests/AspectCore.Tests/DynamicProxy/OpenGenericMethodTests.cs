using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.ObjectPool;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class OpenGenericMethodTests : DynamicProxyTestBase
    {
        [Fact]
        public void OpenGenericProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ObjectPoolProvider, DefaultObjectPoolProvider>();

        }


        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => next(ctx));
        }
    }
}