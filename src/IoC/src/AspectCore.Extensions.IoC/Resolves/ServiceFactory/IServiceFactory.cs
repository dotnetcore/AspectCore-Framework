using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal interface IServiceFactory
    {
        ServiceKey ServiceKey { get; }

        ServiceDefinition ServiceDefinition { get; }

        object Invoke(IServiceResolver serviceResolver);
    }
}