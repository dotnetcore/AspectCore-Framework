using System;

namespace AspectCore.Lite.DependencyInjection
{
    public interface ISupportProxyService
    {
        object GetService(Type serviceType, object originalServiceInstance);
    }
}
