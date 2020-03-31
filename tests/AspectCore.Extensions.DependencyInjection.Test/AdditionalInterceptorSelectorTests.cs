using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore1.Extensions.DependencyInjection.Test
{
    public class AdditionalInterceptorSelectorTests
    {
        [Fact]
        public void ImplementationMethod_Test()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, Service>();
            var provider = services.BuildServiceContextProvider();
            var service = provider.GetService<IService>();
            var val = service.GetValue("le");
            Assert.Equal("lemon", val);
        }

        public class Intercept : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                context.Parameters[0] = "lemon";
                return context.Invoke(next);
            }
        }

        public interface IService
        {
            string GetValue(string val);
        }

        public class Service : IService
        {
            [Intercept]
            public string GetValue(string val)
            {
                return val;
            }
        }
    }
}
