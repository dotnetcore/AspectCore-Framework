using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using AspectCore.Lite.DependencyInjection;
using System;
using System.Reflection;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal sealed class SupportProxyService : ISupportProxyService
    {
        private readonly IServiceProvider supportOriginalServiceProvider;
        private readonly IProxyActivator proxyActivator;

        public SupportProxyService(ISupportOriginalService supportOriginalServiceProvider , IProxyActivator proxyActivator)
        {
            this.supportOriginalServiceProvider = supportOriginalServiceProvider;
            this.proxyActivator = proxyActivator;
        }

        public object GetService(Type serviceType)
        {
            ExceptionHelper.ThrowArgumentNull(serviceType , nameof(serviceType));
            var serviceTypeInfo = serviceType.GetTypeInfo();
            if (serviceTypeInfo.IsClass)
            {
                return proxyActivator.CreateClassProxy(serviceType , supportOriginalServiceProvider.GetOriginalService(serviceType) , serviceTypeInfo.GetInterfaces());
            }
            else if (serviceTypeInfo.IsInterface)
            {
                return proxyActivator.CreateInterfaceProxy(serviceType , supportOriginalServiceProvider.GetOriginalService(serviceType) , serviceTypeInfo.GetInterfaces());
            }
            else
            {
                return supportOriginalServiceProvider.GetService(serviceType);
            }
        }
    }
}
