using AspectCore.Abstractions.Resolution.Test.Fakes;
using Xunit;

namespace AspectCore.Abstractions.Resolution.Test
{
    public class InterceptorInjectorTest
    {
        [Fact]
        public void Inject_Test()
        {
            var configuration = new AspectConfiguration();
           
            var interceptor = new InjectedInterceptor();

            Assert.Null(interceptor.Configuration);

            var interceptorInjector = new InterceptorInjector(new InstanceServiceProvider(configuration));

            interceptorInjector.Inject(interceptor);

            Assert.NotNull(interceptor.Configuration);

            Assert.Equal(interceptor.Configuration, configuration);
        }

        [Fact]
        public void Inject_NoSetAccessor_Test()
        {
            var configuration = new AspectConfiguration();

            var interceptor = new InjectedInterceptor();

            Assert.Null(interceptor.ConfigurationWithNoSet);

            var interceptorInjector = new InterceptorInjector(new InstanceServiceProvider(configuration));

            interceptorInjector.Inject(interceptor);

            Assert.Null(interceptor.ConfigurationWithNoSet);
        }

        [Fact]
        public void Inject_NoFromServicesAttribute_Test()
        {
            var configuration = new AspectConfiguration();

            var interceptor = new InjectedInterceptor();

            Assert.Null(interceptor.ConfigurationWithNoFromServicesAttribute);

            var interceptorInjector = new InterceptorInjector(new InstanceServiceProvider(configuration));

            interceptorInjector.Inject(interceptor);

            Assert.Null(interceptor.ConfigurationWithNoFromServicesAttribute);
        }
    }
}
