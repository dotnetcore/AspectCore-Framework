using System;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectActivator
    {
        T Invoke<T>(object targetInstance, object proxyInstance, Type serviceType, string method, params object[] args);

        Task<T> InvokeAsync<T>(object targetInstance, object proxyInstance, Type serviceType, string method, params object[] args);
    }
}
