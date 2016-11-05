namespace AspectCore.Lite.DependencyInjection.Test.Classes
{
    public interface ILogger
    {
        [CustomeInterceptor]
        void Info();
    }
}