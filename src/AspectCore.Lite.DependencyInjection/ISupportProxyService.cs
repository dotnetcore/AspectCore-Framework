using System;

namespace AspectCore.Lite.DependencyInjection
{
    public interface ISupportProxyService : IServiceProvider
    {
        object OriginalServiceInstance { get; set; }
    }
}
