using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    internal sealed class RuntimeAspectContext : AspectContext
    {
        private IServiceProvider _serviceProvider;
        private IDictionary<string, object> _data;
        private bool _disposedValue = false;

        internal Action<AspectContext> Disposing { get; set; }

        public override IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    throw new NotSupportedException("The current context does not support IServiceProvider.");
                }

                return _serviceProvider;
            }
        }

        public override IDictionary<string, object> Items
        {
            get
            {
                return _data ??
                    (_data = new Dictionary<string, object>());
            }
        }

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

        public RuntimeAspectContext(IServiceProvider serviceProvider, ITargetDescriptor target, IProxyDescriptor proxy, IParameterCollection parameters, IParameterDescriptor returnParameter)
        {
            _serviceProvider = serviceProvider;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ReturnParameter = returnParameter ?? throw new ArgumentNullException(nameof(returnParameter));
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
                _data.TryGetValue(key, out object value);

                var disposable = value as IDisposable;

                disposable?.Dispose();

                _data.Remove(key);
            }

            _disposedValue = true;
        }
    }
}