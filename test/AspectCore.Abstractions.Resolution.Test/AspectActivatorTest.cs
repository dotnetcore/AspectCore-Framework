using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Resolution.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Abstractions.Resolution.Test
{
    public class AspectActivatorTest
    {
        [Fact]
        public async Task InvokeAsync_Test()
        {
            var configuration = new AspectConfiguration();
            var serviceProvider = new InstanceServiceProvider(null);
            var activator = new AspectActivator(serviceProvider, new AspectBuilder(), new InterceptorMatcher(configuration), new InterceptorInjector(serviceProvider));

            activator.InitializeMetaData(typeof(ITargetService),
                    MethodInfoHelpers.GetMethod<Func<ITargetService, int, int>>((m, v) => m.Add(v)),
                    MethodInfoHelpers.GetMethod<Func<TargetService, int, int>>((m, v) => m.Add(v)),
                    MethodInfoHelpers.GetMethod<Func<ProxyService, int, int>>((m, v) => m.Add(v)));

            var input = 0;

            var result = await activator.InvokeAsync<int>(new TargetService(), new ProxyService(), input);

            Assert.Equal(result, input + 1);
        }

        [Fact]
        public void InvokeA_Test()
        {
            var configuration = new AspectConfiguration();
            var serviceProvider = new InstanceServiceProvider(null);
            var activator = new AspectActivator(serviceProvider, new AspectBuilder(), new InterceptorMatcher(configuration), new InterceptorInjector(serviceProvider));

            activator.InitializeMetaData(typeof(ITargetService),
                    MethodInfoHelpers.GetMethod<Func<ITargetService, int, int>>((m, v) => m.Add(v)),
                    MethodInfoHelpers.GetMethod<Func<TargetService, int, int>>((m, v) => m.Add(v)),
                    MethodInfoHelpers.GetMethod<Func<ProxyService, int, int>>((m, v) => m.Add(v)));

            var input = 0;

            var result =  activator.Invoke<int>(new TargetService(), new ProxyService(), input);

            Assert.Equal(result, input + 1);
        }
    }
}
