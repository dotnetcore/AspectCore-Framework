using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Autofac;
using AspectCore.Lite.Abstractions;

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
            builder.RegisterType<TaskRepository>().As<ITaskRepository>();
            builder.RegisterType<TaskRepository>().AsSelf();
            IContainer container = builder.Build();
            ITaskRepository repository = container.Resolve<ITaskRepository>();
            var aaa= container.Resolve<TaskRepository>();
        }
    }
}
