using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal sealed class ServiceScopeAccessor : IServiceScopeAccessor
    {
        private static readonly AsyncLocal<IServiceScope> ServiceScopeLocal = new AsyncLocal<IServiceScope>();

        public IServiceScope CurrentServiceScope
        {
            get => ServiceScopeLocal.Value;
            set => ServiceScopeLocal.Value = value;
        }
    }
}
