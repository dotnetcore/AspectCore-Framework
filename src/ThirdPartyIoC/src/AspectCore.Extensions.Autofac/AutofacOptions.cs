using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Extensions.Configuration;

namespace AspectCore.Extensions.Autofac
{
    public class AutofacOptions
    {
        public ICollection<IInterceptorFactory> InterceptorFactories { get; }
        public ICollection<Func<MethodInfo, bool>> NonAspectPredicates { get; }

        public AutofacOptions()
        {
            InterceptorFactories = new List<IInterceptorFactory>();
            NonAspectPredicates = new List<Func<MethodInfo, bool>>();
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
