using AspectCore.Lite.Abstractions;
using System;

namespace AspectCore.Lite.DynamicProxy
{
    internal sealed class NanoAspectContext : AspectContext
    {
        public override IServiceProvider ServiceProvider
        {
            get
            {
                throw new NotImplementedException("The current context does not support this call.");
            }
        }

        public NanoAspectContext(IServiceProvider serviceProvider, TargetDescriptor target, ProxyDescriptor proxy, ParameterCollection parameters, ReturnParameterDescriptor returnParameter) : base(serviceProvider, target, proxy, parameters, returnParameter)
        {
        }
    }
}
