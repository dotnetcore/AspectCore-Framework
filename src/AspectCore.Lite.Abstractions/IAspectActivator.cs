using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectActivator
    {
        void InitializationMetaData(Type serviceType, MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod);

        T Invoke<T>(object targetInstance, object proxyInstance, params object[] paramters);

        T InvokeAsync<T>(object targetInstance, object proxyInstance, params object[] paramters);
    }
}
