namespace AspectCore.Extensions.DependencyInjection
{
    public interface ITransientServiceAccessor<T>
    {
        T Value { get; }

        T RequiredValue { get; }
    }
}
