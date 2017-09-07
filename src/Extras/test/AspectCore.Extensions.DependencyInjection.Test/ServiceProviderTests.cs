using AspectCore.Extensions.Test.Fakes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using AspectCore.Abstractions;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class ServiceProviderTests
    {
        [Fact]
        public void ServiceProvider_GetService_IsDynamically_Tests()
        {
            var services = new ServiceCollection();
            services.AddAspectCore();
            services.AddTransient<IService, Service>();
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);
            var proxyService = proxyServiceProvider.GetService<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void ServiceProvider_GetService_WithParameter_Tests()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, Service>();
            services.AddTransient<IController, Controller>();
            services.AddAspectCore();
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);
            var proxyService = proxyServiceProvider.GetService<IService>();

            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));

            var proxyController = proxyServiceProvider.GetService<IController>();
            Assert.Equal(proxyService.Get(100), proxyController.Execute());
        }

        [Fact]
        public void SupportOriginalService_Test()
        {
            var services = new ServiceCollection();
            services.AddAspectCore();
            services.AddTransient<IService, Service>();
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);
            var originalServiceProvider = proxyServiceProvider.GetService<IRealServiceProvider>();
            var service = originalServiceProvider.GetService<IService>();
            Assert.False(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.NotEqual(service.Get(1), service.Get(1));
        }

        [Fact]
        public void SupportOriginalServiceWithParameter_Test()
        {
            var services = new ServiceCollection();
            services.AddAspectCore();
            services.AddTransient<IService, Service>();
            services.AddTransient<IController, Controller>();
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);
            var originalServiceProvider = proxyServiceProvider.GetService<IRealServiceProvider>();

            var proxyService = originalServiceProvider.GetService<IService>();

            Assert.False(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));

            var proxyController = originalServiceProvider.GetService<IController>();

            Assert.False(proxyController.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.False(proxyController.Service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));

            Assert.NotEqual(proxyService.Get(100), proxyController.Execute());
        }

        [Fact]
        public void ServiceProvider_GetService_WithInterceptor_Tests()
        {
            var services = new ServiceCollection();
            services.AddAspectCore();
            services.AddTransient<IService, Service>();
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);

            var aspectValidator = proxyServiceProvider.GetService<IAspectValidatorBuilder>().Build();

            foreach (var method in typeof(IService).GetTypeInfo().DeclaredMethods)
            {
                var i = aspectValidator.Validate(method);
            }

            var interceptors = proxyServiceProvider.GetService<IInterceptorProvider>();

            foreach (var method in typeof(IService).GetTypeInfo().DeclaredMethods)
            {
                var iis = interceptors.GetInterceptors(method);
            }
            var proxyService = proxyServiceProvider.GetService<IService>();
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void ServiceProvider_ImplementationInstance_Test()
        {
            var services = new ServiceCollection();
            services.AddAspectCore();
            services.AddSingleton<IService>(new Service());
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);
            var proxyService = proxyServiceProvider.GetService<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void ServiceProvider_ImplementationFactory_Test()
        {
            var services = new ServiceCollection();
            services.AddAspectCore();
            services.AddSingleton<IService>(s => new Service());
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);
            var proxyService = proxyServiceProvider.GetService<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }
    }
}
