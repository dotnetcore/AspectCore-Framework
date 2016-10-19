using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.Test
{
    public class AspectExecutorTest : IDependencyInjection
    {
        public void ExecuteAsync_Test()
        {
           
        }




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
                throw new NotImplementedException();
            }

            public Task<object> ExecuteAsyncWithResult()
            {
                throw new NotImplementedException();
            }

            public void ExecuteSynchronously()
            {
                throw new NotImplementedException();
            }

            public object ExecuteSynchronouslyWithResult()
            {
                throw new NotImplementedException();
            }
        }

        public class AspectExecutorTestServiceProxy : IAspectExecutorTestService
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly IAspectExecutorTestService _serviceInstance;

            public AspectExecutorTestServiceProxy(IServiceProvider serviceProvider, IAspectExecutorTestService serviceInstance)
            {
                _serviceInstance = serviceInstance;
                _serviceProvider = serviceProvider;
            }
            public Task ExecuteAsync()
            {
                var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
                return aspectExecutor.ExecuteAsync<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteAsync");
            }

            public Task<object> ExecuteAsyncWithResult()
            {
                var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
                return aspectExecutor.ExecuteAsync<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteAsyncWithResult");
            }

            public void ExecuteSynchronously()
            {
                var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
                aspectExecutor.ExecuteSynchronously<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteSynchronously");
            }

            public object ExecuteSynchronouslyWithResult()
            {
                var aspectExecutor = _serviceProvider.GetRequiredService<IAspectExecutor>();
                return aspectExecutor.ExecuteSynchronously<object>(_serviceInstance , this , typeof(IAspectExecutorTestService) , "ExecuteSynchronouslyWithResult");
            }
        }
    }
}