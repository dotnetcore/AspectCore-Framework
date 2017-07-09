using System;
using System.Linq;
using AspectCore.Abstractions;
using AbstractAspectContext = AspectCore.Abstractions.AspectContext;

namespace AspectCore.Core
{
    [NonAspect]
    internal sealed class AspectContext : AbstractAspectContext
    {
        private AspectScopeManager _aspectScopeManager;
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

        public AspectContext(
            IServiceProvider serviceProvider, 
            ITargetDescriptor target, 
            IProxyDescriptor proxy, 
            IParameterCollection parameters, 
            IParameterDescriptor returnParameter,
            AspectScopeManager aspectScopeManager)
        {
            _serviceProvider = serviceProvider;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ReturnParameter = returnParameter ?? throw new ArgumentNullException(nameof(returnParameter));
            _aspectScopeManager = aspectScopeManager;
            _aspectScopeManager.AddScope(this);
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

            _aspectScopeManager.Remove(this);

            _disposedValue = true;
        }
    }
}