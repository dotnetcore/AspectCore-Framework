using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.Configuration
{
    public class AspectCoreOptions
    {
        public ICollection<IInterceptorFactory> InterceptorFactories { get; } = new List<IInterceptorFactory>();

        public ICollection<Func<MethodInfo, bool>> NonAspectPredicates { get; } = new List<Func<MethodInfo, bool>>();

        public IServiceCollection InternalServices { get; } = new ServiceCollection();
    }
}