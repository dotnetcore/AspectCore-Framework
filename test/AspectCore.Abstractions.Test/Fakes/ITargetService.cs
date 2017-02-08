namespace AspectCore.Abstractions.Internal.Test.Fakes
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
