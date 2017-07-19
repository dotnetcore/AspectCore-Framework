using AspectCore.Abstractions;
using AspectCore.Extensions.Configuration.Test.Fakes;
using AspectCore.Extensions.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AspectCore.Abstractions.Internal;

namespace AspectCore.Extensions.Configuration.Test
{
    public class ServiceInterceptorAttributeTest
    {
        [Fact]
        public void Test()
        {
            var factory = new ProxyFactoryBuilder().UseServiceProvider(GetServiceProvider).Build();
            var userService = factory.CreateInterfaceProxy<IUserService>(new UserService(), Type.EmptyTypes);
            userService.GetName();
        }

        private IServiceProvider GetServiceProvider(IAspectConfigure configure)
        {
            var provider = new SimpleServiceProvider(configure);
            provider.AddService(typeof(Logger), () => new Logger());
            provider.AddService(typeof(LoggerInterceptorAttribute), () => new LoggerInterceptorAttribute((Logger)provider.GetService(typeof(Logger))));
            return provider;
        }
    }
}