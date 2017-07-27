namespace AspectCore.Extensions.DependencyInjection
{
    public interface ITransientServiceAccessor<out T>
    {
        T Value { get; }

        T RequiredValue { get; }
    }
}
