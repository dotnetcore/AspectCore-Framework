using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class DefaultAspectContext<T> : AspectContext
    {
        private IServiceProvider serviceProvider;
        private IDictionary<string, object> items;
        private bool disposedValue = false;

        public override IServiceProvider ServiceProvider
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

        public override IDictionary<string, object> Items { get { return items ?? (items = new Dictionary<string, object>()); } }

        public override ParameterCollection Parameters
        {
            get;
        }

        public override ParameterDescriptor ReturnParameter
        {
            get;
        }

        public override TargetDescriptor Target
        {
            get;
        }

        public override ProxyDescriptor Proxy
        {
            get;
        }

        public DefaultAspectContext(IServiceProvider provider, TargetDescriptor target, ProxyDescriptor proxy, ParameterCollection parameters, ReturnParameterDescriptor returnParameter)
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

        public DefaultAspectContext(IServiceProvider provider, AspectActivatorContext context)
            : this(provider,
                 new TargetDescriptor(context.TargetInstance, context.ServiceMethod, context.ServiceType, context.TargetMethod, context.TargetInstance.GetType()),
                 new ProxyDescriptor(context.ProxyInstance, context.ProxyMethod, context.ProxyInstance.GetType()),
                 new ParameterCollection(context.Parameters, context.ServiceMethod.GetParameters()),
                 new ReturnParameterDescriptor(default(T), context.ServiceMethod.ReturnParameter))
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposedValue)
            {
                return;
            }

            if (!disposing)
            {
                return;
            }

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

            disposedValue = true;
        }
    }
}