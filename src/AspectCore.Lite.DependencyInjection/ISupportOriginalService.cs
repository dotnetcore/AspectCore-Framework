using System;

namespace AspectCore.Lite.DependencyInjection
{
    public interface ISupportOriginalService : IServiceProvider
    {
        IServiceProvider OriginalServiceProvider { get; }
    }
}