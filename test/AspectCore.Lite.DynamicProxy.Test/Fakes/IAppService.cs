namespace AspectCore.Lite.DynamicProxy.Test.Fakes
{
    [Increment]
    public interface IAppService
    {
        int Run(int arg);

        T Run1<T>(T arg);
    }

    [Increment]
    public interface IAppService<T>
    {
        int Run(int arg);

        T Run1(T arg);
    }

    public interface IAppService1
    {
        int Run(int arg);

        T Run1<T>(T arg);
    }

    public interface IAppService1<T>
    {
        int Run(int arg);

        T Run1(T arg);
    }
}
