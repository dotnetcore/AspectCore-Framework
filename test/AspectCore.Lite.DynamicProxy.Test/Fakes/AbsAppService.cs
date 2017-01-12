namespace AspectCore.Lite.DynamicProxy.Test.Fakes
{
    [Decrement]
    public abstract class AbsAppService : IAppService
    {
        public abstract int Run(int arg);

        public abstract T Run1<T>(T arg);
    }
}
