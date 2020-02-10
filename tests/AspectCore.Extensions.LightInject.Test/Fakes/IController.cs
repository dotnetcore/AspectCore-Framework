namespace AspectCoreTest.LightInject.Fakes
{
    [CacheInterceptor]
    public interface IController
    {
        IService Service { get; }
        Model Execute();
    }
}
