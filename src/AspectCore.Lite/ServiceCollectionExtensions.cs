
using AspectCore.Lite.Abstractions.Internal;
using AspectCore.Lite.Core;
using AspectCore.Lite.Extensions;
using AspectCore.Lite.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider BuildAspectServiceProvider(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.BuildServiceProvider();
        }

        public static IServiceCollection AddAspects(this IServiceCollection services, 
            Action<IAspectCollection, IAspectFactory> configure = default(Action<IAspectCollection, IAspectFactory>))
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            IAspectCollection aspectCollection = new AspectCollection();
            IAspectFactory aspectFactory = new AspectFactory();

            configure?.Invoke(aspectCollection, aspectFactory);

            services.AddSingleton(aspectFactory);
            services.AddSingleton(aspectCollection);
            services.AddTransient<IAspectContextFactoryFactory, AspectContextFactoryFactory>();

            aspectCollection.ForEach(aspect => AddAspect(services, aspect));

            return services;
        }

        private static void AddAspect(IServiceCollection services, Aspect aspect)
        {
            if (aspect.Advice != null)
            {
                object advice = aspect.Advice;
                services.AddSingleton(advice.GetType(), advice);
            }
            else if (aspect.AdviceType != null)
            {
                services.AddTransient(aspect.AdviceType);
            }
            else
            {
                throw new ArgumentException("Aspect information description is not complete.");
            }
        }
    }
}