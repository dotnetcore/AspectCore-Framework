using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectActivator
    {
        void InitializeMetaData(Type serviceType, MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod);

        T Invoke<T>(object targetInstance, object proxyInstance, params object[] paramters);

        Task<T> InvokeAsync<T>(object targetInstance, object proxyInstance, params object[] paramters);
    }
}
