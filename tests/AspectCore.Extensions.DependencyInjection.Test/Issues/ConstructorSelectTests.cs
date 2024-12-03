using System;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test.Issues
{
    //https://github.com/dotnetcore/AspectCore-Framework/issues/313
    public class ConstructorSelectTests
    {
        public interface IInteriorService
        {
            string SayHello();
        }
        public interface IExternalService
        {
            string SayHello();
        }

        public class InteriorServiceImpl : IInteriorService
        {
            [CustomInterceptor]
            public string SayHello() => "Hello Interior!";
        }
        public class ExternalServiceImpl : IExternalService
        {
            private StringBuilder output = new StringBuilder();
            private readonly IInteriorService? _service1;

            public ExternalServiceImpl(IInteriorService service1) : this()
            {
                _service1 = service1;
                output.AppendLine("ExternalServiceImpl(IParentService)");
            }
            public ExternalServiceImpl()
            {
                output.AppendLine("ExternalServiceImpl()");
            }

            [CustomInterceptor]
            public string SayHello()
            {
                output.AppendLine("Hello External!");
                output.AppendLine(_service1?.SayHello());
                return output.ToString();
            }
        }

        public class CustomInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next) => await context.Invoke(next);
    }

        [Fact]
        public void ConstructorSelectWithInterceptor_Test()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IInteriorService, InteriorServiceImpl>();
            services.AddTransient<IExternalService, ExternalServiceImpl>();
            services.ConfigureDynamicProxy(option =>
            {
                option.Interceptors.AddTyped<CustomInterceptor>();
            });
            IServiceProvider serviceProvider = services.BuildDynamicProxyProvider();
            IExternalService service2 = serviceProvider.GetRequiredService<IExternalService>();
            string output = service2.SayHello();
            
        }
    }
}