namespace AspectCore.Injector
{
    public interface IPropertyInjector
    {
        void Invoke(object implementation);
    }
}
