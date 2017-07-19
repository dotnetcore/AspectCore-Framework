using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.Configuration
{
    public class AspectCoreOptions
    {
        public ICollection<IInterceptorFactory> InterceptorFactories { get; } 

        public ICollection<Func<MethodInfo, bool>> NonAspectPredicates { get; } = new List<Func<MethodInfo, bool>>();

        public IServiceCollection InternalServices { get; }

        public AspectCoreOptions(IServiceCollection services)
        {
            InternalServices = services ?? new ServiceCollection();
            InterceptorFactories = new List<IInterceptorFactory>();
            NonAspectPredicates= new List<Func<MethodInfo, bool>>();
            NonAspectPredicates.
                AddObjectVMethod().
                AddSystem().
                AddAspNetCore().
                AddEntityFramework().
                AddOwin().
                AddPageGenerator();
        }
    }
}