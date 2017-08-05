namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IPropertyInjector
    {
        void Invoke(object implementation);
    }
}
