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
            var provider = this.BuildProxyServicePrivoder(
                services => services.AddTransient<ITaskService, TaskService>().AddTransient<ILogger, Logger>());
            var proxyService = provider.GetService<ITaskService>();
            var logger = proxyService.logger;
            Assert.NotNull(proxyService);
            Assert.IsAssignableFrom<ITaskService>(proxyService);
            Assert.IsNotType<TaskService>(proxyService);
        }
    }
}