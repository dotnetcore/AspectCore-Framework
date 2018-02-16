using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Injector;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class ConstructorInjectionTest : InjectorTestBase
    {
        [Fact]
        public void Constructor_Inject()
        {
            var userService = ServiceResolver.Resolve<IUserService>();
            Assert.NotNull(userService);
            Assert.NotNull(userService.Repository);
            Assert.NotNull(userService.Logger);
        }

        [Fact]
        public void Equal()
        {
            var userService1 = ServiceResolver.Resolve<IUserService>();
            var userService2 = ServiceResolver.Resolve<IUserService>();
            Assert.NotEqual(userService1, userService2);
            Assert.Equal(userService1.Repository, userService2.Repository);
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.AddType<ILogger, Logger>(Lifetime.Singleton);
            serviceContainer.AddType(typeof(IRepository<>), typeof(Repository<>), Lifetime.Scoped);
            serviceContainer.AddType<IUserService, UserService>();
        }
    }
}
