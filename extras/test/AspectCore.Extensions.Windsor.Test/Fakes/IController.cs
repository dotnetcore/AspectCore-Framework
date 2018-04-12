namespace AspectCore.Extensions.Windsor.Test.Fakes
{
    [CacheInterceptor]
    public interface IController
    {
        ICacheService Service { get; }
        Model Execute();
    }
}
