using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// NOTE: This namespace must NOT match "AspectCore.Extensions.*" because that's
// excluded by default NonAspectPredicates.
namespace AspectCoreTest.InternalClass
{
    public class TransactionalInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            context.ReturnValue = "intercepted";
        }
    }

    public interface ICustomService
    {
        [TransactionalInterceptor]
        string Todo1();

        string Todo2();
    }

    // Internal implementation class — scenario from issue #274
    internal class InternalCustomService : ICustomService
    {
        public string Todo1() => "original1";
        public string Todo2() => "original2";
    }

    // Public implementation class for comparison
    public class PublicCustomService : ICustomService
    {
        public string Todo1() => "original1";
        public string Todo2() => "original2";
    }

    public class InternalClassTests
    {
        [Fact]
        public void Internal_Class_Proxy_Is_Generated()
        {
            var services = new ServiceCollection();
            services.AddScoped<ICustomService, InternalCustomService>();
            services.ConfigureDynamicProxy();

            var provider = services.BuildDynamicProxyProvider();
            var service = provider.GetRequiredService<ICustomService>();

            var typeName = service.GetType().FullName;
            Console.WriteLine($"Internal type: {typeName}");

            // Verify proxy IS generated
            Assert.StartsWith("AspectCore.DynamicGenerated", typeName);

            // Todo1 has the interceptor attribute — should be intercepted
            Assert.Equal("intercepted", service.Todo1());

            // Todo2 is NOT intercepted — should call the real implementation
            // This is the method that used to throw MethodAccessException
            Assert.Equal("original2", service.Todo2());
        }

        [Fact]
        public void Public_Class_Proxy_Is_Generated()
        {
            var services = new ServiceCollection();
            services.AddScoped<ICustomService, PublicCustomService>();
            services.ConfigureDynamicProxy();

            var provider = services.BuildDynamicProxyProvider();
            var service = provider.GetRequiredService<ICustomService>();

            var typeName = service.GetType().FullName;
            Console.WriteLine($"Public type: {typeName}");

            Assert.StartsWith("AspectCore.DynamicGenerated", typeName);
            Assert.Equal("intercepted", service.Todo1());
            Assert.Equal("original2", service.Todo2());
        }
    }
}
