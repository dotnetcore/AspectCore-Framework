namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IServiceInstanceAccessor
    {
        object ServiceInstance { get; }
    }

    [NonAspect]
    public interface IServiceInstanceAccessor<TService>
    {
        TService ServiceInstance { get; }
    }
}
