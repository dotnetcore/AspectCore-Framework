using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class AspectContext : IAspectContext
    {
        private IServiceProvider serviceProvider;
        private IDictionary<string, object> items;

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


        public IDictionary<string, object> Items { get { return items ?? (items = new Dictionary<string, object>()); } }


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


        public void Dispose()
        {
            if (items == null)
            {
                return;
            }

            foreach (var key in items.Keys.ToArray())
            {
                object value = null;

                items.TryGetValue(key, out value);

                var disposable = value as IDisposable;

                disposable?.Dispose();

                items.Remove(key);
            }
        }
    }
}