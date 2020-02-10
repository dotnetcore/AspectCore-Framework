namespace AspectCoreTest.LightInject.Fakes
{
    public interface IService
    {
        [CacheInterceptor]
        Model Get(int id);
    }
}
