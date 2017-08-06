using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal interface IServiceFactory
    {
        ServiceKey ServiceKey { get; }

        object Invoke(IServiceResolver resolver);
    }
}