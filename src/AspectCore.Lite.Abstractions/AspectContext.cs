using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public abstract class AspectContext
    {
        public virtual IServiceProvider ServiceProvider { get; }

        public virtual TargetDescriptor Target { get; }

        public virtual ProxyDescriptor Proxy { get; }

        public virtual ParameterCollection Parameters { get; }

        public virtual ParameterDescriptor ReturnParameter { get; }

        public virtual Object AspectData { get; set; }

        public AspectContext(IServiceProvider serviceProvider, TargetDescriptor target, ProxyDescriptor proxy, ParameterCollection parameters, ReturnParameterDescriptor returnParameter)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (returnParameter == null)
            {
                throw new ArgumentNullException(nameof(returnParameter));
            }

            ServiceProvider = serviceProvider;
            Target = target;
            Proxy = proxy;
            Parameters = parameters;
            ReturnParameter = returnParameter;
        }
    }
}