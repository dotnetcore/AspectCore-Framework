using System;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // sample for property injection
            var services = new ServiceCollection();
//            services.AddTransient<ILogger, ConsoleLogger>();
//            services.AddTransient<ISampleService, SampleService>();
            var container = services.ToServiceContainer();
            container.AddType<ILogger, ConsoleLogger>();
            container.AddType<ISampleService, SampleService>();
            var serviceResolver = container.Build();
            var sampleService = serviceResolver.Resolve<ISampleService>();
            sampleService.Invoke();
            Console.ReadKey();
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
        [FromContainer]
        public ILogger Logger { get; set; }
        
        public void Invoke()
        {
           Logger?.Info("sample service invoke.");
        }
    }
}