using System;

namespace AspectCore.DependencyInjection
{
    public static class ServiceContainerBuildExtensions
    {
        public static IServiceResolver Build(this IServiceContext serviceContext)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            return new ServiceResolver(serviceContext);
        }
    }
}