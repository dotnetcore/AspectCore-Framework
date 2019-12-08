using System;
using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Autofac.Sample;
using AspectCore.DependencyInjection;
using Autofac;

namespace AspectCoreExtensions.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceContext = new ServiceContext();
            serviceContext.AddType<ITaskService, TaskService>();

            var containerBuilder = new ContainerBuilder();

            //调用Populate扩展方法在Autofac中注册已经注册到ServiceContainer中的服务（如果有）。注：此方法调用应在RegisterDynamicProxy之前
            containerBuilder.Populate(serviceContext);

            var configuration = serviceContext.Configuration;

            //调用RegisterDynamicProxy扩展方法在Autofac中注册动态代理服务和动态代理配置
            containerBuilder.RegisterDynamicProxy(configuration, config =>
             {
                 config.Interceptors.AddTyped<MethodExecuteLoggerInterceptor>(Predicates.ForService("*Service"));
             });

            var container = containerBuilder.Build();

            var taskService = container.Resolve<ITaskService>();

            taskService.Run();
        }
    }

    public interface ITaskService
    {
        bool Run();
    }

    public class TaskService : ITaskService
    {
        public bool Run()
        {
            return true;
        }
    }
}