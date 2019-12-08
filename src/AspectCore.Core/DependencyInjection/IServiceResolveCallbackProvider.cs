using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    internal interface IServiceResolveCallbackProvider
    {
        IServiceResolveCallback[] ServiceResolveCallbacks { get; }
    }
}