using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class ParameterInjectionTest : DynamicProxyTestBase
    {
        [Fact]
        public void Parameter_Intercept()
        {
            var service = ProxyGenerator.CreateClassProxy<AppService>();
            Assert.Throws<AspectInvocationException>(() =>
            {
                service.Run(null);
            });
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddTyped<EnableParameterAspectInterceptor>();
        }
    }

    public class AppService
    {
        public virtual void Run([NotNull]string name)
        {
        } 
    }
}