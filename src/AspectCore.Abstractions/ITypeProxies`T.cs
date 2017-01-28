namespace AspectCore.Abstractions
{
    public interface ITypeProxies<T> : ITypeProxies
    {
        new T ServiceInstance { get; }
    }
}
