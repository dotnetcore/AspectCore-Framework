using AspectCore.Lite.DependencyInjection;
using AspectCore.Lite.Extensions;
using AspectCore.Lite.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAspectLite(this IServiceCollection services)
        {
            ExceptionUtilities.ThrowArgumentNull(services , nameof(services));

            var aspectService = ServiceCollectionUtilities.CreateAspectLiteServices();
            aspectService.ForEach(d => services.TryAdd(d));

            return services;
        }
    }
}
