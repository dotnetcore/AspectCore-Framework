using AspectCore.Lite.Abstractions;
using System;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectContext : Abstractions.AspectContext
    {
        public AspectContext(IServiceProvider serviceProvider, TargetDescriptor target, ProxyDescriptor proxy, ParameterCollection parameters, ReturnParameterDescriptor returnParameter) 
            : base(serviceProvider, target, proxy, parameters, returnParameter)
        {
        }
    }
}
