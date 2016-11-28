using System;
using AspectCore.Lite.Abstractions;

namespace AspectCore.Lite.DependencyInjection.Test
{
    public interface IProxyServiceTest
    {
    }

    public class ProxyServiceTest : IProxyServiceTest
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IProxyServiceTest proxyService;

        public ProxyServiceTest(IServiceProvider serviceProvider, IOriginalServiceProvider originalServiceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.proxyService = (IProxyServiceTest)originalServiceProvider.GetService(typeof(IProxyServiceTest));
        }
    }
}