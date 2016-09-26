using AspectCore.Lite.Core.Descriptors;

namespace AspectCore.Lite.Core
{
    public interface IMethodInvoker
    {
        void InjectionParameters(ParameterCollection parameterCollection);
        object Invoke();
    }
}
