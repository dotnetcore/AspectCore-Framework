using System;
using System.Linq;
using AspectCore.Abstractions;
using AbstractAspectContext = AspectCore.Abstractions.AspectContext;

namespace AspectCore.Core
{
    internal sealed class AspectContext : AbstractAspectContext
    {
        private IServiceProvider _serviceProvider;
        private AspectDictionary _data;
        private bool _disposedValue = false;

        public override IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    throw new NotImplementedException("The current context does not support IServiceProvider.");
                }

                return _serviceProvider;
            }
        }

        public override AspectDictionary Items { get { return _data ?? (_data = new AspectDictionary()); } }

        public override IParameterCollection Parameters
        {
            get;
        }

        public override IParameterDescriptor ReturnParameter
        {
            get;
        }

        public override ITargetDescriptor Target
        {
            get;
        }

        public override IProxyDescriptor Proxy
        {
            get;
        }

        public AspectContext(IServiceProvider provider, ITargetDescriptor target, IProxyDescriptor proxy, IParameterCollection parameters, IParameterDescriptor returnParameter)
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

            var realServiceProvider = provider as IRealServiceProvider;
            _serviceProvider = realServiceProvider ?? (IServiceProvider)provider?.GetService(typeof(IRealServiceProvider));
            Target = target;
            Proxy = proxy;
            Parameters = parameters;
            ReturnParameter = returnParameter;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (!disposing)
            {
                return;
            }

            if (_data == null)
            {
                return;
            }

            foreach (var key in _data.Keys.ToArray())
            {
                object value = null;

                _data.TryGetValue(key, out value);

                var disposable = value as IDisposable;

                disposable?.Dispose();

                _data.Remove(key);
            }

            _disposedValue = true;
        }
    }
}