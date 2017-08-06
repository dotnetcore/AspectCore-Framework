using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC
{
    public static class ServiceContainerExtensions
    {
        public static IServiceResolver Build(this IServiceContainer services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            return new ServiceResolver(services);
        }
    }
}
