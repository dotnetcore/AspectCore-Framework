using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Resolution.Test.Fakes;
using NSubstitute;
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

            var input = 0;

            var activatorContext = Substitute.For<AspectActivatorContext>();
            activatorContext.Parameters.Returns(new object[] { input });
            activatorContext.ServiceType.Returns(typeof(ITargetService));
            activatorContext.ServiceMethod.Returns(MethodExtensions.GetMethod<Func<ITargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetMethod.Returns(MethodExtensions.GetMethod<Func<TargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.ProxyMethod.Returns(MethodExtensions.GetMethod<Func<ProxyService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetInstance.Returns(new TargetService());
            activatorContext.ProxyInstance.Returns(new ProxyService());
    
            var result = await activator.InvokeAsync<int>(activatorContext);

            Assert.Equal(result, input + 1);
        }

        [Fact]
        public void InvokeA_Test()
        {
            var configuration = new AspectConfiguration();
            var serviceProvider = new InstanceServiceProvider(null);
            var activator = new AspectActivator(serviceProvider, new AspectBuilder(), new InterceptorMatcher(configuration), new InterceptorInjector(serviceProvider));

            var input = 0;

            var activatorContext = Substitute.For<AspectActivatorContext>();
            activatorContext.Parameters.Returns(new object[] { input });
            activatorContext.ServiceType.Returns(typeof(ITargetService));
            activatorContext.ServiceMethod.Returns(MethodExtensions.GetMethod<Func<ITargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetMethod.Returns(MethodExtensions.GetMethod<Func<TargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.ProxyMethod.Returns(MethodExtensions.GetMethod<Func<ProxyService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetInstance.Returns(new TargetService());
            activatorContext.ProxyInstance.Returns(new ProxyService());

            var result = activator.Invoke<int>(activatorContext);

            Assert.Equal(result, input + 1);
        }
    }
}
