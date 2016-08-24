using AspectCore.Lite.Core;
using AspectCore.Lite.Extensions;
using AspectCore.Lite.Internal;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        //public static IServiceProvider BuildAspectServiceProvider(this IServiceCollection services)
        //{
        //    if (services == null) throw new ArgumentNullException(nameof(services));

        //    return services.BuildServiceProvider();
        //}

        public static IServiceCollection AddAspects(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<IAspectContextFactoryProvider, AspectContextFactoryProvider>();

            return WavingProxy(services);
        }


        internal static IServiceCollection WavingProxy(IServiceCollection collection)
        {
            int count = collection.Count;
            for (int index = 0; index < count; index++)
            {
                ServiceDescriptor descriptor = collection[index];
                if (descriptor.ImplementationType == null) continue;



                collection.ReplaceAt(index, null);
            }
            return collection;
        }
    }
}