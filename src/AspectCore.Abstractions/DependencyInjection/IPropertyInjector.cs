using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface IPropertyInjector
    {
        void Invoke(object implementation);
    }
}
