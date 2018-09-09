using System.Collections.Generic;

namespace AspectCore.Injector
{
    internal interface IServiceResolveCallbackProvider
    {
        IServiceResolveCallback[] ServiceResolveCallbacks { get; }
    }
}