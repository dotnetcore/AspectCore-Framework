using AspectCore.Lite.Abstractions;
using AspectCore.Lite.DynamicProxy.Generators;
using AspectCore.Lite.DynamicProxy.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy.Container
{
    public static class ContainerHelper
    {
        public static IEnumerable<ServiceDescription> GetAspectServiceDescriptions()
        {
            yield return ServiceDescription.Description<IAspectActivator, AspectActivator>(Lifetime.Transient);
            yield return ServiceDescription.Description<IAspectBuilder, AspectBuilder>(Lifetime.Transient);
            yield return ServiceDescription.Description<IInterceptorInjector, InterceptorInjector>(Lifetime.Scoped);
            yield return ServiceDescription.Description<IAspectValidator, AspectValidator>(Lifetime.Singleton);
            yield return ServiceDescription.Description<IInterceptorMatcher, InterceptorMatcher>(Lifetime.Singleton);
            yield return ServiceDescription.Description<IInterceptorCollection, InterceptorCollection>(Lifetime.Singleton);
        }

        public static IInterceptorCollection GetInterceptorTable()
        {
            return new InterceptorCollection();
        }

        public static Type CreateAspectType(Type serviceType, Type implementationType, IServiceProvider serviceProvider)
        {
            return new AspectTypeGenerator(serviceType, implementationType, serviceProvider).CreateType();
        }
    }
}
