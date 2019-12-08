using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.DependencyInjection;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    public class AspectCoreFacility : IFacility
    {
        private readonly IAspectConfiguration _aspectConfiguration;
        private IKernel _kernel;

        public AspectCoreFacility(IAspectConfiguration aspectConfiguration)
        {
            _aspectConfiguration = aspectConfiguration ?? new AspectConfiguration();
        }

        public void Init(IKernel kernel, IConfiguration facilityConfig)
        {
            if (!kernel.HasComponent(typeof(IServiceProvider)))
                kernel.Register(Component.For<IServiceProvider>().ImplementedBy<WindsorServiceResolver>().LifestyleSingleton());
            kernel.Register(
                Component.For(typeof(ManyEnumerable<>)).ImplementedBy(typeof(ManyEnumerable<>)).LifestyleTransient(),
                Component.For<IProxyGenerator>().ImplementedBy<ProxyGenerator>().LifestyleScoped(),
                Component.For<IProxyTypeGenerator>().ImplementedBy<ProxyTypeGenerator>().LifestyleSingleton(),
                Component.For<IInterceptorSelector>().ImplementedBy<ConfigureInterceptorSelector>().LifestyleSingleton(),
                Component.For<IInterceptorSelector>().ImplementedBy<AttributeInterceptorSelector>().LifestyleSingleton(),
                Component.For<IAspectBuilderFactory>().ImplementedBy<AspectBuilderFactory>().LifestyleSingleton(),
                Component.For<IInterceptorCollector>().ImplementedBy<InterceptorCollector>().LifestyleSingleton(),
                Component.For<IAspectContextFactory>().ImplementedBy<AspectContextFactory>().LifestyleSingleton(),
                Component.For<IAspectCachingProvider>().ImplementedBy<AspectCachingProvider>().LifestyleSingleton(),
                Component.For<IAspectExceptionWrapper>().ImplementedBy<AspectExceptionWrapper>().LifestyleSingleton(),
                Component.For<IAspectActivatorFactory>().ImplementedBy<AspectActivatorFactory>().LifestyleSingleton(),
                Component.For<IAspectValidatorBuilder>().ImplementedBy<AspectValidatorBuilder>().LifestyleSingleton(),
                Component.For<IPropertyInjectorFactory>().ImplementedBy<PropertyInjectorFactory>().LifestyleSingleton(),
                Component.For<IParameterInterceptorSelector>().ImplementedBy<ParameterInterceptorSelector>().LifestyleSingleton(),
                Component.For<IAdditionalInterceptorSelector>().ImplementedBy<AttributeAdditionalInterceptorSelector>().LifestyleSingleton(),
                Component.For<IAspectConfiguration>().Instance(_aspectConfiguration).LifestyleSingleton()
            );
            kernel.Register(Component.For<DynamicProxyInterceptor>());
            kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
            kernel.Resolver.AddSubResolver(new CompatibleCollectionResolver(kernel));
            _kernel = kernel;
        }

        public void Terminate()
        {
            _kernel.ComponentModelCreated -= Kernel_ComponentModelCreated;
        }

        private void Kernel_ComponentModelCreated(ComponentModel model)
        {
            var aspectValidator = new AspectValidatorBuilder(_aspectConfiguration).Build();
            if (aspectValidator.Validate(model.Implementation, false) || model.Services.Any(x => aspectValidator.Validate(x, true)))
            {
                model.Interceptors.AddIfNotInCollection(InterceptorReference.ForType<DynamicProxyInterceptor>());
            }
        }
    }
}