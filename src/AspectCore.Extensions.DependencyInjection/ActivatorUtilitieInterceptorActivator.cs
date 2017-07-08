using System;
using AspectCore.Abstractions;
using AspectCore.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal sealed class ActivatorUtilitieInterceptorActivator : ITypedInterceptorActivator
    {
        private readonly IRealServiceProvider _serviceProvider;

        public ActivatorUtilitieInterceptorActivator(IRealServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IInterceptor CreateInstance(Type interceptorType, object[] args)
        {
            return (IInterceptor)ActivatorUtilities.CreateInstance(_serviceProvider, interceptorType, args);
        }
    }
}