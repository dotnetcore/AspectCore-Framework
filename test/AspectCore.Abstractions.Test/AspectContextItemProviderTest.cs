using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Abstractions.Internal;
using AspectCore.Abstractions.Internal.Test.Fakes;
using AspectCore.Abstractions.Test.Fakes;
using Xunit;

namespace AspectCore.Abstractions.Test
{
    public class AspectContextItemProviderTest
    {
        [Fact]
        public void AspectContextItemProvider_Get_Test()
        {
            var configure = new AspectConfigure();
            var serviceProvider = new InstanceServiceProvider(null);
            var activator = new AspectActivator(serviceProvider,
                new AspectBuilderProvider(new InterceptorSelector(new InterceptorMatcher(configure), new InterceptorInjectorProvider(serviceProvider, new PropertyInjectorSelector()))));

            var generator = new ProxyGenerator(new AspectValidator(configure));
            var proxyType = generator.CreateClassProxyType(typeof(TestService), typeof(TestService));
            var proxyInstance = (TestService)Activator.CreateInstance(proxyType, new InstanceServiceProvider(activator));

            proxyInstance.Test1();
            proxyInstance.Test2(0);
        }

        [ItemInterceptor]
        public class TestService
        {
            public virtual void Test1() { }

            public virtual void Test2(int arg, IAspectContextItemProvider aspectContextItemProvider = null)
            {
                Assert.Equal(aspectContextItemProvider.Items["key"], "ItemInterceptor");
            }
        }
    }
}
