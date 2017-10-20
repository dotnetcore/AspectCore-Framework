using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;

namespace AspectCore.Tests.DynamicProxy
{
    public class DynamicProxyTestBase
    {
        protected IProxyGenerator ProxyGenerator { get; }

        public DynamicProxyTestBase()
        {
            var builder = new ProxyGeneratorBuilder();
            builder.Configure(Configure);
            ProxyGenerator = builder.Build();
        }

        protected virtual void Configure(IAspectConfiguration configuration) { }
    }
}
