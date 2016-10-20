using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.Test.Fakes;
using Xunit;

namespace AspectCore.Lite.Test
{
    public class AspectExecutorTest : IDependencyInjection
    {
        [Fact]
        public async Task ExecuteAsync_Test()
        {
            var serviceProvider = this.BuildServiceProvider();
            var aspectExecutor = serviceProvider.GetRequiredService<IAspectExecutor>();
            var targetInstance = new AspectExecutorTestServiceTarget();
            var proxyInstance = new AspectExecutorTestServiceProxy(serviceProvider , targetInstance);
            var result = aspectExecutor.ExecuteAsync<int>(targetInstance , proxyInstance , typeof(IAspectExecutorTestService) , nameof(targetInstance.ExecuteAsync));
            Assert.Equal(await result , await Task.FromResult(0));
        }


    }

    [EmptyInterceptor]
    public interface IAspectExecutorTestService
    {
        void ExecuteSynchronously();

        Task ExecuteAsync();

        object ExecuteSynchronouslyWithResult();

        Task<object> ExecuteAsyncWithResult();
    }

    public class AspectExecutorTestServiceTarget : IAspectExecutorTestService
    {
        public Task ExecuteAsync()
        {
            return Task.FromResult(0);
        }

        public Task<object> ExecuteAsyncWithResult()
        {
            return Task.FromResult<object>(0);
        }

        public void ExecuteSynchronously()
        {
        }

        public object ExecuteSynchronouslyWithResult()
        {
            return 0;
        }
    }

    public class AspectExecutorTestServiceProxy : IAspectExecutorTestService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAspectExecutorTestService _serviceInstance;

        public AspectExecutorTestServiceProxy(IServiceProvider serviceProvider , IAspectExecutorTestService serviceInstance)
        {
            _serviceInstance = serviceInstance;
            _serviceProvider = serviceProvider;
        }
        Task IAspectExecutorTestService.ExecuteAsync()
        {
            var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
            return aspectExecutor.ExecuteAsync<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteAsync");
        }

        Task<object> IAspectExecutorTestService.ExecuteAsyncWithResult()
        {
            var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
            return aspectExecutor.ExecuteAsync<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteAsyncWithResult");
        }

        void IAspectExecutorTestService.ExecuteSynchronously()
        {
            var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
            aspectExecutor.ExecuteSynchronously<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteSynchronously");
        }

        object IAspectExecutorTestService.ExecuteSynchronouslyWithResult()
        {
            var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
            return aspectExecutor.ExecuteSynchronously<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteSynchronouslyWithResult");
        }
    }
}