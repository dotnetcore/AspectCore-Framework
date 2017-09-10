using System.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    public interface IParameterInterceptorSelector
    {
        IParameterInterceptor[] Select(ParameterInfo parameter);
    }
}