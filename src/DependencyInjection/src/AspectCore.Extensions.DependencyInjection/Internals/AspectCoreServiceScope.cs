using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal class AspectCoreServiceScope : IServiceScope
    {
        private readonly IServiceProvider _scopedProvider;
        private readonly IServiceScopeAccessor _serviceScopeAccessor;

        public AspectCoreServiceScope(IServiceProvider scopedProvider, IServiceScopeAccessor serviceScopeAccessor)
        {
            _scopedProvider = scopedProvider;
            _serviceScopeAccessor = serviceScopeAccessor;
            _serviceScopeAccessor.CurrentServiceScope = this;
        }

        public IServiceProvider ServiceProvider => _scopedProvider;

        public void Dispose()
        {
            (_scopedProvider as IDisposable)?.Dispose();
            _serviceScopeAccessor.CurrentServiceScope = null;
        }
    }
}