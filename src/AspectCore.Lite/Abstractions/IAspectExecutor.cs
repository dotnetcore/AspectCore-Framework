using System;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectExecutor
    {
        TResult Execute<TResult>(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args);

        Task<TResult> ExecuteAsync<TResult>(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args);
    }
}
