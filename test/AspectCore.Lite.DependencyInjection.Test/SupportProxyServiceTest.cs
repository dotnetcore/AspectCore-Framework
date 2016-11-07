using System.Collections.Generic;
using AspectCore.Lite.DependencyInjection.Test.Classes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Lite.DependencyInjection.Test
{
    public class SupportProxyServiceTest : IDependencyInjection
    {
        [Fact]
        public void Proxy_Test()
        {
            var provider = this.BuildServiceProvider(
                services => services.AddTransient<ITaskService, TaskService>().AddTransient<ILogger, Logger>());

            var supportProxyService = provider.GetService<ISupportProxyService>();

            var proxyTaskService = (ITaskService)supportProxyService.GetService(typeof(ITaskService),
                provider.GetService<ITaskService>());

            Assert.NotNull(proxyTaskService);
            Assert.IsAssignableFrom<ITaskService>(proxyTaskService);
            Assert.IsNotType<TaskService>(proxyTaskService);

            //var logger = proxyTaskService.logger;
            //Assert.IsNotType<Logger>(logger);
            //Assert.IsAssignableFrom<ILogger>(logger);
        }

        [Fact]
        public void NonProxy_Test()
        {
            var provider = this.BuildServiceProvider(
                services => services.AddTransient<TaskService>().AddTransient<ILogger, Logger>());

            var supportProxyService = provider.GetService<ISupportProxyService>();

            var proxyTaskService = (TaskService)supportProxyService.GetService(typeof(TaskService),
                provider.GetService<TaskService>());

            Assert.NotNull(proxyTaskService);
            Assert.IsType<TaskService>(proxyTaskService);

            var logger = proxyTaskService.logger;
            Assert.IsNotType<Logger>(logger);
            Assert.IsAssignableFrom<ILogger>(logger);
        }

        [Fact]
        public void Instance_Null_Test()
        {
            var provider = this.BuildServiceProvider();

            var supportProxyService = provider.GetService<ISupportProxyService>();
            var proxyService = supportProxyService.GetService(typeof(ITaskService), null);
            Assert.Null(proxyService);
        }

        [Fact]
        public void OpenIEnumerable_Proxy_Test()
        {
            var provider = this.BuildServiceProvider(
                services => services.AddTransient<ITaskService, TaskService>().AddTransient<ITaskService, TaskService>().AddTransient<ILogger, Logger>());

            var supportProxyService = provider.GetService<ISupportProxyService>();
            var proxyServices = supportProxyService.GetService(typeof(IEnumerable<ITaskService>),
                provider.GetServices<ITaskService>());

            Assert.NotNull(proxyServices);
            var enumerableServices = proxyServices as IEnumerable<ITaskService>;
            Assert.NotNull(enumerableServices);
            foreach (var proxyService in enumerableServices)
            {
                //Assert.IsAssignableFrom<ITaskService>(proxyService);
                //var logger = proxyService.logger;
                
            }
        }
    }
}