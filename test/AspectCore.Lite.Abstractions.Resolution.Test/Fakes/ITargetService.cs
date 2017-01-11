namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public interface ITargetService
    {
        [Increment]
        int Add(int value);
    }

    [Increment]
    public interface ITargetService<T>
    {

        [Increment]
        T Add(T value);
    }
}
