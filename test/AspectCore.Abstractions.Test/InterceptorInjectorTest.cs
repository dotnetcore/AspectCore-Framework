using AspectCore.Abstractions.Internal.Test.Fakes;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class InterceptorInjectorTest
    {
        [Fact]
        public void Inject_Test()
        {
            var Configure = new AspectConfigure();
           
            var interceptor = new InjectedInterceptor();

            Assert.Null(interceptor.Configure);

            var interceptorInjector = new InterceptorInjector(new InstanceServiceProvider(Configure),new PropertyInjectorSelector().SelectPropertyInjector(typeof(InjectedInterceptor)));

            interceptorInjector.Inject(interceptor);

            Assert.NotNull(interceptor.Configure);

            Assert.Equal(interceptor.Configure, Configure);
        }

        [Fact]
        public void Inject_NoSetAccessor_Test()
        {
            var Configure = new AspectConfigure();

            var interceptor = new InjectedInterceptor();

            Assert.Null(interceptor.ConfigureWithNoSet);

            var interceptorInjector = new InterceptorInjector(new InstanceServiceProvider(Configure), new PropertyInjectorSelector().SelectPropertyInjector(typeof(InjectedInterceptor)));

            interceptorInjector.Inject(interceptor);

            Assert.Null(interceptor.ConfigureWithNoSet);
        }

        [Fact]
        public void Inject_NoFromServicesAttribute_Test()
        {
            var Configure = new AspectConfigure();

            var interceptor = new InjectedInterceptor();

            Assert.Null(interceptor.ConfigureWithNoFromServicesAttribute);

            var interceptorInjector = new InterceptorInjector(new InstanceServiceProvider(Configure), new PropertyInjectorSelector().SelectPropertyInjector(typeof(InjectedInterceptor)));

            interceptorInjector.Inject(interceptor);

            Assert.Null(interceptor.ConfigureWithNoFromServicesAttribute);
        }
    }
}
