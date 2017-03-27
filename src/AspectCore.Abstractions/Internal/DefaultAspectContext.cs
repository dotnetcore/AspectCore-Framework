using System;
using System.Linq;

namespace AspectCore.Abstractions.Internal
{
    public sealed class DefaultAspectContext<T> : AspectContext
    {
        private IServiceProvider serviceProvider;
        private DynamicDictionary data;
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

        public override DynamicDictionary Data { get { return data ?? (data = new DynamicDictionary()); } }

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

            var originalServiceProvider = provider as IOriginalServiceProvider;
            serviceProvider = originalServiceProvider ?? (IServiceProvider)provider?.GetService(typeof(IOriginalServiceProvider));
            Target = target;
            Proxy = proxy;
            Parameters = parameters;
            ReturnParameter = returnParameter;
        }

        public DefaultAspectContext(IServiceProvider provider, AspectActivatorContext context)
            : this(provider,
                 new TargetDescriptor(context.TargetInstance, context.ServiceMethod, context.ServiceType, context.TargetMethod, context.TargetInstance?.GetType() ?? context.TargetMethod.DeclaringType),
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

            if (data == null)
            {
                return;
            }

            foreach (var key in data.Keys.ToArray())
            {
                object value = null;

                data.TryGetValue(key, out value);

                var disposable = value as IDisposable;

                disposable?.Dispose();

                data.Remove(key);
            }

            disposedValue = true;
        }
    }
}