using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Autofac;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.DependencyInjection;

namespace AspectCore.Lite.Autofac.Test
{
    public class ContainerBuilderExtensionsTest
    {
        [Fact]
        public void RegisterAspectLite_Test()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAspectLite();
            IContainer container = builder.Build();
            Assert.True(container.IsRegistered<IServiceProvider>());
            Assert.True(container.IsRegistered<IJoinPoint>());
            Assert.True(container.IsRegistered<IAspectContextFactory>());
            Assert.True(container.IsRegistered<IAspectExecutor>());
        }

        [Fact]
        public void InterfaceProxy_Test()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAspectLite();
            //builder.Register(c => new Logger()).InstancePerLifetimeScope();
            //builder.RegisterType<TaskRepository>().As<ITaskRepository>();
            builder.RegisterType<Logger>().AsSelf().InstancePerLifetimeScope();
            //IContainer container = builder.Build();
            //ITaskRepository repository = container.Resolve<ITaskRepository>();
            //var logger = repository.Logger;
            IContainer container = builder.Build();
            var rootProviderWrapper = container.Resolve<IServiceProviderWrapper>();
            var log1 = rootProviderWrapper.GetOriginalService<Logger>();
            var log2 = rootProviderWrapper.GetOriginalService<Logger>();
            var proxyLogger = container.Resolve<Logger>();
            Assert.Equal(log1, log2);
            var liftTime = container.BeginLifetimeScope();
            var liftTimeProviderWrapper = liftTime.Resolve<IServiceProviderWrapper>();
            var log3 = liftTimeProviderWrapper.GetOriginalService<Logger>();
            var log4 = liftTimeProviderWrapper.GetOriginalService<Logger>();
            Assert.Equal(log3, log4);
            Assert.NotEqual(log1, log4);
            //Assert.Equal(rootProviderWrapper, liftTimeProviderWrapper);
        }
    }
}
