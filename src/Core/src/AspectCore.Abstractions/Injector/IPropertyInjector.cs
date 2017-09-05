using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface IPropertyInjector
    {
        void Invoke(object implementation);
    }
}
