namespace AspectCore.Extensions.Windsor.Test.Fakes
{
    [CacheInterceptor]
    public interface IController
    {
        IService Service { get; }
        Model Execute();
    }
}
