using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1) sample for property injection
            var services = new ServiceCollection();
            services.AddTransient<ILogger, ConsoleLogger>();
            services.AddTransient<ISampleService, SampleService>();
            var serviceProvider = services.BuildServiceContextProvider();
            //            var container = services.ToServiceContext();
            //            container.AddType<ILogger, ConsoleLogger>();
            //            container.AddType<ISampleService, SampleService>();
            //            var serviceResolver = container.Build();
            //            var sampleService = serviceResolver.Resolve<ISampleService>();
            var sampleService = serviceProvider.GetService<ISampleService>();
            sampleService.Invoke();

            // 2) smoke: SG AOP + registry discovery + class proxy
            var aopServices = new ServiceCollection();
            aopServices.AddTransient<Demo.InterceptedService>();
            aopServices.ConfigureDynamicProxyEngine(o =>
            {
                o.Engine = ProxyEngine.SourceGenerator;
                o.Strict = true;
            });
            aopServices.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddTyped<Demo.MethodExecuteLoggerInterceptor>(Predicates.ForService("*Service"));
            });

            var aopProvider = aopServices.BuildDynamicProxyProvider();

            var v = aopProvider.GetRequiredService<IAspectValidatorBuilder>().Build();
            Console.WriteLine($"Validate(InterceptedService, strict=true): {v.Validate(typeof(Demo.InterceptedService), true)}");
            Console.WriteLine($"IProxyTypeGenerator: {aopProvider.GetRequiredService<IProxyTypeGenerator>().GetType().FullName}");

            var intercepted = aopProvider.GetRequiredService<Demo.InterceptedService>();
            Console.WriteLine($"InterceptedService runtime type: {intercepted.GetType().FullName}");
            Console.WriteLine($"IsProxyType: {ReflectionUtils.IsProxyType(intercepted.GetType().GetTypeInfo())}");
            intercepted.Invoke();

            if (args != null && args.Length > 0 && args[0] == "--wait")
            {
                Console.ReadKey();
            }
        }
    }

    public interface ILogger
    {
        void Info(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    public interface ISampleService
    {
        void Invoke();
    }

    public class SampleService : ISampleService
    {
        [FromServiceContext]
        public ILogger Logger { get; set; }
        
        public void Invoke()
        {
           Logger?.Info("sample service invoke.");
        }
    }

}

namespace Demo
{
    using System;
    using System.Threading.Tasks;
    using AspectCore.DynamicProxy;

    public sealed class MethodExecuteLoggerInterceptor : AbstractInterceptor
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Console.WriteLine($"[Interceptor] before: {context.ImplementationMethod.Name}");
            return next(context);
        }
    }

    [AspectCoreGenerateProxy]
    public class InterceptedService
    {
        public virtual void Invoke()
        {
            Console.WriteLine("InterceptedService.Invoke");
        }
    }
}
