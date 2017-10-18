using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Injector;
using AspectCore.Tests.Injector;
using AspectCore.DynamicProxy;
using System.Threading.Tasks;
using AspectCore.Configuration;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class InterceptorPropertyInjectionTest: InjectorTestBase
    {
        [Fact]
        public void Test()
        {
            var service = ServiceResolver.Resolve<FakeService>();
            Assert.Equal("lemon", service.Foo("le"));
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.Configuration.Interceptors.AddTyped<Intercept>(Predicates.ForService("*FakeService"));
            serviceContainer.AddType<FakeService>();
            serviceContainer.AddDelegate<FakeProperty>(r => new FakeProperty { Val = "lemon" });
            base.ConfigureService(serviceContainer);
        }

        public class FakeService
        {
            public virtual string Foo(string val)
            {
                return val;
            }
        }

        public class Intercept : AbstractInterceptor
        {
            [FromContainer]
            public FakeProperty FakeProperty { get; set; }
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                context.Parameters[0] = FakeProperty?.Val;
                return context.Invoke(next);
            }
        }
    }

    
}
