using System;

namespace AspectCore.Injector
{
    public static class ServiceContainerBuildExtensions
    {
        public static IServiceResolver Build(this IServiceContainer serviceContainer)
        {
            if (serviceContainer == null)
            {
                throw new ArgumentNullException(nameof(serviceContainer));
            }
            return new ServiceResolver(serviceContainer);
        }
    }
}