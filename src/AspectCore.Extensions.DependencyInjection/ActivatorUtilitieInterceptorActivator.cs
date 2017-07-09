using System;
using AspectCore.Abstractions;
using AspectCore.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal sealed class ActivatorUtilitieInterceptorActivator : ITypedInterceptorActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ActivatorUtilitieInterceptorActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IInterceptor CreateInstance(Type interceptorType, object[] args)
        {
            return (IInterceptor)ActivatorUtilities.CreateInstance(_serviceProvider, interceptorType, args);
        }
    }
}