using System;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // sample for property injection
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
        [FromServiceContext]
        public ILogger Logger { get; set; }
        
        public void Invoke()
        {
           Logger?.Info("sample service invoke.");
        }
    }
}