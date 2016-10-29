using System;

namespace AspectCore.Lite.DependencyInjection
{
    public interface ISupportSingletonService : IServiceProvider, IDisposable
    {
    }
}