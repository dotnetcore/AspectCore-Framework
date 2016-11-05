namespace AspectCore.Lite.DependencyInjection.Test.Classes
{
    [CustomeInterceptor]
    public interface ITaskService
    {
        ILogger logger { get; }

        void Run();
    }
}