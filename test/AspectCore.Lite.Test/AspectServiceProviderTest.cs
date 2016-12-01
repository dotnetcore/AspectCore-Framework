using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.DependencyInjection;
using AspectCore.Lite.Test.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Lite.Test
{
    public class AspectServiceProviderTest
    {
        [Fact]
        public void BuildTest()
        {
            var services = new ServiceCollection();
            services.AddTransient<ITaskService, DefaultTaskService>();
            var provider = services.BuildAspectServiceProvider();
            var taskService = provider.GetService<ITaskService>();
            taskService.Run();
            taskService.Run();
        }

        [Fact]
        public void Scoped_Test()
        {
            var services = new ServiceCollection();
            services.AddScoped<ITaskService, DefaultTaskService>();
            var provider = services.BuildAspectServiceProvider().GetService<IServiceScope>().ServiceProvider;
            var taskService1 = provider.GetService<ITaskService>();
            var taskService2 = provider.GetService<ITaskService>();
            Assert.Equal(taskService1, taskService2);

            var originalProvider = provider.GetService<IOriginalServiceProvider>();
            Assert.Equal(originalProvider.GetService(typeof(ITaskService)),originalProvider.GetService(typeof(ITaskService)));

            var field_serviceInstance =
                Expression.Lambda<Func<object>>(Expression.Field(Expression.Constant(taskService1),
                    "proxyfield#serviceInstance")).Compile()();
            Assert.Equal(originalProvider.GetService(typeof(ITaskService)),field_serviceInstance);
        }

        [EmptyInterceptor]
        public interface ITaskService
        {
            void Run();
        }

        public class DefaultTaskService : ITaskService
        {
            public void Run()
            {
            }
        }
    }
}