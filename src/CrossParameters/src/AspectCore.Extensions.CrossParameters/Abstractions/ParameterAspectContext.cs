using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters
{
    public sealed class ParameterAspectContext : IDisposable
    {
        private IDictionary<string, object> items;

        public AspectContext AspectContext { get; }

        public IParameterDescriptor Parameter { get; }

        public IServiceProvider ServiceProvider { get { return AspectContext.ServiceProvider; } }

        public IDictionary<string, object> Items { get { return items ?? (items = new Dictionary<string, object>()); } }

        internal ParameterAspectContext(AspectContext aspectContext, IParameterDescriptor parameter)
        {
            AspectContext = aspectContext;
            Parameter = parameter;
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
