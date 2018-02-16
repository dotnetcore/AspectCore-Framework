using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface ITransientServiceAccessor<T> where T : class
    {
        T Value { get; }
    }
}