using AspectCore.Lite.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test
{
    public interface IDependencyInjection
    {
    }

    public static class IDependencyInjectionExtensions
    {
        public static IServiceProvider BuildServiceProvider(this IDependencyInjection di , Action<IServiceCollection> action = null)
        {
            IServiceCollection services = new ServiceCollection().TryAddAspectCoreLite();

            //services.AddAspectLite();
            action?.Invoke(services);

            return services.BuildServiceProvider();
        }
    }
}
