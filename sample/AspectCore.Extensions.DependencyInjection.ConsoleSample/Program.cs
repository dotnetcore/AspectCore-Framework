using System;
using System.Linq;
using System.Reflection;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // sample for property injection
            //            var services = new ServiceCollection();
            //            services.AddTransient<ILogger, ConsoleLogger>();
            //            services.AddTransient<ISampleService, SampleService>();
            //            var serviceProvider = services.BuildServiceContextProvider();
            ////            var container = services.ToServiceContext();
            ////            container.AddType<ILogger, ConsoleLogger>();
            ////            container.AddType<ISampleService, SampleService>();
            ////            var serviceResolver = container.Build();
            ////            var sampleService = serviceResolver.Resolve<ISampleService>();
            //            var sampleService = serviceProvider.GetService<ISampleService>();
            //            sampleService.Invoke();
            //            Console.ReadKey();
            //var obj = new TestService();
            //var methodInfo = obj.GetType().GetTypeInfo().GetMethod("Update");
            //var parameterInfo = methodInfo.GetParameters().FirstOrDefault();
            //var customAttributeDatas = parameterInfo.CustomAttributes.ToArray();

            //// 小于8个参数
            //var methodInfo2 = obj.GetType().GetTypeInfo().GetMethod("Update2");
            //var parameterInfo2 = methodInfo2.GetParameters().FirstOrDefault();
            //var customAttributeDatas2 = parameterInfo2.CustomAttributes.ToArray();

            //var methodInfo3 = obj.GetType().GetTypeInfo().GetMethod("Update3");
            //var parameterInfo3 = methodInfo3.GetParameters().FirstOrDefault();
            //var customAttributeDatas3 = parameterInfo3.CustomAttributes.ToArray();

            //var customAttributeTypedArguments = customAttributeDatas2[0].ConstructorArguments;
            Issue305.Start();
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