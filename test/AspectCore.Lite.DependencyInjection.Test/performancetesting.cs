using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Activators;
using AspectCore.Lite.DependencyInjection.Test.Classes;
using Xunit;
using  Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection.Test
{
    public class performancetesting : IDependencyInjection
    {
        [Fact]
        public void Test()
        {
            var provider =
                this.BuildServiceProvider(
                    services => services.AddTransient<ITaskService, TaskService>().AddTransient<ILogger, Logger>());
            var proxyProvider = AspectServiceProviderFactory.Create(provider);

            IProxyActivator proxyActivator = new ProxyActivator();

            int count = 100000;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                proxyActivator.CreateInterfaceProxy<ILogger>(new Logger());
            }
            Console.WriteLine(sw.Elapsed);
            sw.Restart();
            for (int i = 0; i < count; i++)
            {
                provider.GetService<ITaskService>();
            }
            Console.WriteLine(sw.Elapsed);
            sw.Restart();
            for (int i = 0; i < count; i++)
            {
                proxyProvider.GetService<ITaskService>();
            }
            Console.WriteLine(sw.Elapsed);

            sw.Restart();
            for (int i = 0; i < count; i++)
            {
                ActivatorUtilities.CreateInstance<TaskService>(provider);
            }
            Console.WriteLine(sw.Elapsed);
        }
    }
}