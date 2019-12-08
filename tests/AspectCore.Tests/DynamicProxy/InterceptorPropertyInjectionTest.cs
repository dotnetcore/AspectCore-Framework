using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
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

        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.Configuration.Interceptors.AddTyped<Intercept>(Predicates.ForService("*FakeService"));
            serviceContext.AddType<FakeService>();
            serviceContext.AddDelegate<FakeProperty>(r => new FakeProperty { Val = "lemon" });
            base.ConfigureService(serviceContext);
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
            [FromServiceContext]
            public FakeProperty FakeProperty { get; set; }
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                context.Parameters[0] = FakeProperty?.Val;
                return context.Invoke(next);
            }
        }
    }

    
}
