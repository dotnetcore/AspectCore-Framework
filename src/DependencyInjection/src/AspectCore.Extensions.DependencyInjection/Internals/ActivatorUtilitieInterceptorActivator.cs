using System;
using AspectCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal sealed class ActivatorUtilitieInterceptorActivator : IInterceptorActivator
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