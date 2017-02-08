using System;
using System.Threading.Tasks;
using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Internal.Test.Fakes;
using NSubstitute;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class AspectActivatorTest
    {
        [Fact]
        public async Task InvokeAsync_Test()
        {
            var configure = new AspectConfigure();
            var serviceProvider = new InstanceServiceProvider(null);
            var activator = new AspectActivator(serviceProvider,
                new AspectBuilderProvider(new InterceptorSelector(new InterceptorMatcher(configure), new InterceptorInjectorProvider(serviceProvider, new PropertyInjectorSelector()))));
 
             var input = 0;

            var activatorContext = Substitute.For<AspectActivatorContext>();
            activatorContext.Parameters.Returns(new object[] { input });
            activatorContext.ServiceType.Returns(typeof(ITargetService));
            activatorContext.ServiceMethod.Returns(ReflectionExtensions.GetMethod<Func<ITargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetMethod.Returns(ReflectionExtensions.GetMethod<Func<TargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.ProxyMethod.Returns(ReflectionExtensions.GetMethod<Func<ProxyService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetInstance.Returns(new TargetService());
            activatorContext.ProxyInstance.Returns(new ProxyService());
    
            var result = await activator.InvokeAsync<int>(activatorContext);

            Assert.Equal(result, input + 1);
        }

        [Fact]
        public void InvokeA_Test()
        {
            var Configure = new AspectConfigure();
            var serviceProvider = new InstanceServiceProvider(null);
            var activator = new AspectActivator(serviceProvider,
                           new AspectBuilderProvider(new InterceptorSelector(new InterceptorMatcher(Configure), new InterceptorInjectorProvider(serviceProvider, new PropertyInjectorSelector()))));

            var input = 0;

            var activatorContext = Substitute.For<AspectActivatorContext>();
            activatorContext.Parameters.Returns(new object[] { input });
            activatorContext.ServiceType.Returns(typeof(ITargetService));
            activatorContext.ServiceMethod.Returns(ReflectionExtensions.GetMethod<Func<ITargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetMethod.Returns(ReflectionExtensions.GetMethod<Func<TargetService, int, int>>((m, v) => m.Add(v)));
            activatorContext.ProxyMethod.Returns(ReflectionExtensions.GetMethod<Func<ProxyService, int, int>>((m, v) => m.Add(v)));
            activatorContext.TargetInstance.Returns(new TargetService());
            activatorContext.ProxyInstance.Returns(new ProxyService());

            var result = activator.Invoke<int>(activatorContext);

            Assert.Equal(result, input + 1);
        }
    }
}
