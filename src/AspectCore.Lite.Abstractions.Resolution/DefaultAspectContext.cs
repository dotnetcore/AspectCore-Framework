using AspectCore.Lite.Abstractions;
using System;

namespace AspectCore.Lite.Abstractions.Resolution
{
    internal sealed class DefaultAspectContext : AspectContext
    {
        public override IServiceProvider ServiceProvider
        {
            get
            {
                if(base.ServiceProvider == null)
                {
                    throw new NotImplementedException("The current context does not support this call.");
                }

                return base.ServiceProvider;
            }
        }
        public DefaultAspectContext(IServiceProvider serviceProvider, TargetDescriptor target, ProxyDescriptor proxy, ParameterCollection parameters, ReturnParameterDescriptor returnParameter) 
            : base(serviceProvider, target, proxy, parameters, returnParameter)
        {
        }
    }
}
