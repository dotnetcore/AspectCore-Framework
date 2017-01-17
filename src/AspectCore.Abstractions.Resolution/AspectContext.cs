using System;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class AspectContext : IAspectContext
    {
        private readonly IServiceProvider serviceProvider;

        public IServiceProvider ServiceProvider
        {
            get
            {
                if (serviceProvider == null)
                {
                    throw new NotImplementedException("The current context does not support IServiceProvider.");
                }

                return serviceProvider;
            }
        }

        public TargetDescriptor Target { get; }

        public ProxyDescriptor Proxy { get; }

        public ParameterCollection Parameters { get; }

        public ParameterDescriptor ReturnParameter { get; }

        public object AspectData { get; set; }

        public AspectContext(IServiceProvider provider, TargetDescriptor target, ProxyDescriptor proxy, ParameterCollection parameters, ReturnParameterDescriptor returnParameter)
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

            serviceProvider = provider;
            Target = target;
            Proxy = proxy;
            Parameters = parameters;
            ReturnParameter = returnParameter;
        }
    }
}
