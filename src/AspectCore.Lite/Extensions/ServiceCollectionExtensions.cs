using AspectCore.Lite.Extensions;
using AspectCore.Lite.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAspectLite(this IServiceCollection services)
        {
            ExceptionHelper.ThrowArgumentNull(services , nameof(services));

            var aspectService = ServiceCollectionHelper.CreateAspectLiteServices();
            aspectService.ForEach(d => services.TryAdd(d));
            services.AddSingleton(services);

            return services;
        }
    }
}
