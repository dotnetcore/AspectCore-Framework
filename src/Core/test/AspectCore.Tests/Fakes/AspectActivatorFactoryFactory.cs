using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;
using AspectCore.Core;
using AspectCore.Core.DynamicProxy;
using AspectCore.Core.Injector;

namespace AspectCore.Tests.Fakes
{
    class AspectActivatorFactoryFactory
    {
        public static IAspectActivatorFactory Create()
        {
            var serviceProvider = new ServiceProvider();
            var interceptorSelectors = new List<IInterceptorSelector>();
            interceptorSelectors.Add(new ConfigureInterceptorSelector(AspectConfigureProvider.Instance, serviceProvider));
            interceptorSelectors.Add(new MethodInterceptorSelector());
            interceptorSelectors.Add(new TypeInterceptorSelector());
            return new AspectActivatorFactory(new AspectContextFactory(serviceProvider), new AspectBuilderFactory(new InterceptorCollector(interceptorSelectors, new PropertyInjectorFactory(serviceProvider))));
        }
    }
}