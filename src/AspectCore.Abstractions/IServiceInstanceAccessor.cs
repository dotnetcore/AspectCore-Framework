namespace AspectCore.Abstractions
{
    public interface IServiceInstanceAccessor
    {
        object ServiceInstance { get; }
    }

    public interface IServiceInstanceAccessor<TService>
    {
        TService ServiceInstance { get; }
    }
}
