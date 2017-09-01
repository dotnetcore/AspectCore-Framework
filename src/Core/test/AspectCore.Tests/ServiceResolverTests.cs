using System;
using System.Collections.Generic;

using System.Text;
using AspectCore.Core.Injector;
using AspectCore.Abstractions;
using Xunit;

namespace AspectCore.Tests
{
    public class ServiceResolverTests
    {
        [Fact]
        public void CreateInstance()
        {
            var services = new ServiceContainer();
            services.Transients.AddType<IUserService, UserService>();
            services.Transients.AddType(typeof(IRepository<>), typeof(Repository<>));
            var resolver = services.Build();
            var userService = resolver.Resolve<IUserService>();
            var userRepository = resolver.Resolve<IRepository<User>>();
            var userServices = resolver.Resolve<IEnumerable<IUserService>>();
        }
    }

    public interface IUserService { }

    public class UserService : IUserService { }

    public interface IRepository<T> { }

    public class Repository<T> : IRepository<T> { }

    public class User { }
}