using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Injector;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    public class AspectCoreFacility : IFacility
    {
        private IKernel _kernel;

        private readonly IAspectConfiguration _aspectConfiguration;

        public AspectCoreFacility(IAspectConfiguration aspectConfiguration)
        {
            _aspectConfiguration = aspectConfiguration ?? new AspectConfiguration();
        }

        public void Init(IKernel kernel, IConfiguration facilityConfig)
        {
            _kernel = kernel;
            kernel.ComponentModelCreated += Kernel_ComponentModelCreated;

            kernel.Register(
                Component.For<IServiceProvider>().ImplementedBy<WindsorServiceResolver>().LifestyleScoped(),
                Component.For(typeof(ManyEnumerable<>)).ImplementedBy(typeof(ManyEnumerable<>)).LifestyleTransient(),
                Component.For<IProxyGenerator>().ImplementedBy<ProxyGenerator>().LifestyleScoped(),
                Component.For<IServiceResolver>().ImplementedBy<WindsorServiceResolver>().Named("ServiceResolver").LifestyleScoped(),
                Component.For<IProxyTypeGenerator>().ImplementedBy<ProxyTypeGenerator>().LifestyleSingleton(),
                Component.For<IInterceptorSelector>().ImplementedBy<ConfigureInterceptorSelector>().LifestyleSingleton(),
                Component.For<IInterceptorSelector>().ImplementedBy<AttributeInterceptorSelector>().LifestyleSingleton(),
                Component.For<IScopeResolverFactory>().ImplementedBy<WindsorScopeResolverFactory>().LifestyleScoped(),
                Component.For<IAspectBuilderFactory>().ImplementedBy<AspectBuilderFactory>().LifestyleSingleton(),
                Component.For<IInterceptorCollector>().ImplementedBy<InterceptorCollector>().LifestyleSingleton(),
                Component.For<IAspectContextFactory>().ImplementedBy<AspectContextFactory>().LifestyleScoped(),
                Component.For<IAspectCachingProvider>().ImplementedBy<AspectCachingProvider>().LifestyleSingleton(),
                Component.For<IAspectActivatorFactory>().ImplementedBy<AspectActivatorFactory>().LifestyleScoped(),
                Component.For<IAspectValidatorBuilder>().ImplementedBy<AspectValidatorBuilder>().LifestyleSingleton(),
                Component.For<IPropertyInjectorFactory>().ImplementedBy<PropertyInjectorFactory>().LifestyleScoped(),
                Component.For<IParameterInterceptorSelector>().ImplementedBy<ParameterInterceptorSelector>().LifestyleScoped(),
                Component.For<IAdditionalInterceptorSelector>().ImplementedBy<AttributeAdditionalInterceptorSelector>().LifestyleSingleton(),
                Component.For<IAspectConfiguration>().Instance(_aspectConfiguration).LifestyleSingleton()
                );
            kernel.Register(Component.For<AspectCoreInterceptor>());
            kernel.Resolver.AddSubResolver(new CompatibleCollectionResolver(kernel));
        }

        private void Kernel_ComponentModelCreated(ComponentModel model)
        {
            var aspectValidator = new AspectValidatorBuilder(_aspectConfiguration).Build();
            if (aspectValidator.Validate(model.Implementation, false) || model.Services.Any(x => aspectValidator.Validate(x, true)))
            {
                model.Interceptors.AddIfNotInCollection(InterceptorReference.ForType<AspectCoreInterceptor>());
            }
        }

        public void Terminate()
        {
            if (_kernel != null)
            {
                _kernel.ComponentModelCreated -= Kernel_ComponentModelCreated;
            }
        }
    }
}