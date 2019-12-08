namespace AspectCoreTest.Windsor.Fakes
{
    [CacheInterceptor]
    public interface IController
    {
        ICacheService Service { get; }
        Model Execute();
    }
}
