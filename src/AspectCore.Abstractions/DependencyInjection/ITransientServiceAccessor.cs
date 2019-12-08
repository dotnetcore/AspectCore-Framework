using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface ITransientServiceAccessor<T> where T : class
    {
        T Value { get; }
    }
}