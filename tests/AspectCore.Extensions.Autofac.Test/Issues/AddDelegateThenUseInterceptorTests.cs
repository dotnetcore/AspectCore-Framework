using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Autofac;
using Xunit;
using AspectCore.Extensions.Autofac;

namespace AspectCoreTest.Autofac.Issues
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/208
    public class AddDelegateThenUseInterceptorTests
    {
        public interface ITaskService
        {
            int Run();
        }

        public class TaskService : ITaskService
        {
            public int Run() => 1;
        }

        public class TaskServiceWithRef : ITaskService
        {
            private readonly ITaskService _taskService;

            public TaskServiceWithRef(ITaskService taskService)
            {
                _taskService = taskService;
            }

            public int Run() => _taskService.Run();
        }

        public class Interceptor : AbstractInterceptor
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await next(context);
                if (context.ReturnValue is int i)
                    context.ReturnValue = i + 1;
            }
        }

        [Fact]
        public void AddDelegateInAspectCore_UseInterceptorInAspectCore_Test()
        {
            var serviceContext = new ServiceContext();
            serviceContext.AddDelegate<ITaskService, TaskService>(p => new TaskService());
            serviceContext.Configuration.Interceptors.AddTyped<Interceptor>(m => true);

            var container = serviceContext.Build();
            var taskService = container.Resolve<ITaskService>();
            Assert.Equal(2, taskService.Run());
        }

        [Fact]
        public void AddDelegateInAutofac_UseInterceptorInAutofac_Test()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register<ITaskService>(c => new TaskService());
            containerBuilder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddTyped<Interceptor>(m => true);
            });

            var container = containerBuilder.Build();
            var taskService = container.Resolve<ITaskService>();
            Assert.Equal(2, taskService.Run());
        }

        [Fact]
        public void AddDelegateInAspectCore_UseInterceptorInAutofac_Test()
        {
            var serviceContext = new ServiceContext();
            serviceContext.AddDelegate<ITaskService, TaskService>(p => new TaskService());

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceContext);

            containerBuilder.RegisterDynamicProxy(serviceContext.Configuration, config =>
            {
                config.Interceptors.AddTyped<Interceptor>(m => true);
            });

            var container = containerBuilder.Build();
            var taskService = container.Resolve<ITaskService>();
            Assert.Equal(2, taskService.Run());
        }

        [Fact]
        public void AddDelegateInAspectCore_WithRef_UseInterceptorInAutofac_Test()
        {
            var serviceContext = new ServiceContext();
            serviceContext.AddDelegate<ITaskService, TaskServiceWithRef>(p => new TaskServiceWithRef(new TaskService()));

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceContext);

            containerBuilder.RegisterDynamicProxy(serviceContext.Configuration, config =>
            {
                config.Interceptors.AddTyped<Interceptor>(m => true);
            });

            var container = containerBuilder.Build();
            var taskService = container.Resolve<ITaskService>();
            Assert.Equal(2, taskService.Run());
        }

        [Fact]
        public void AddDelegateInAspectCore_WithRefFromContainer_UseInterceptorInAutofac_Test()
        {
            var serviceContext = new ServiceContext();
            serviceContext.AddDelegate<TaskService>(p => new TaskService());
            serviceContext.AddDelegate<ITaskService, TaskServiceWithRef>(p => new TaskServiceWithRef(p.Resolve<TaskService>()));

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceContext);

            containerBuilder.RegisterDynamicProxy(serviceContext.Configuration, config =>
            {
                config.Interceptors.AddTyped<Interceptor>(m => true);
            });

            var container = containerBuilder.Build();
            var taskService = container.Resolve<ITaskService>();
            Assert.Equal(3, taskService.Run()); // Intercept twice
        }
    }
}
