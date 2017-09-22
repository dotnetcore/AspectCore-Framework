using System;
using AspectCore.Configuration;
using Castle.MicroKernel;
using Castle.Windsor;

namespace AspectCore.Extensions.Windsor
{
    public static class FacilityExtensions
    {
        public static IKernel AddAspectCoreFacility(this IKernel Kernel, IAspectConfiguration configuration, Action<IAspectConfiguration> configure = null)
        {
            if (Kernel == null)
            {
                throw new ArgumentNullException(nameof(Kernel));
            }
            var config = configuration ?? new AspectConfiguration();
            configure?.Invoke(config);
            Kernel.AddFacility(new AspectCoreFacility(config));
            return Kernel;
        }

        public static IKernel AddAspectCoreFacility(this IKernel Kernel, Action<IAspectConfiguration> configure = null)
        {
            return AddAspectCoreFacility(Kernel, null, configure);
        }

        public static IWindsorContainer AddAspectCoreFacility(this IWindsorContainer windsorContainer, IAspectConfiguration configuration, Action<IAspectConfiguration> configure = null)
        {
            if (windsorContainer == null)
            {
                throw new ArgumentNullException(nameof(windsorContainer));
            }
            AddAspectCoreFacility(windsorContainer.Kernel, configuration, configure);
            return windsorContainer;
        }

        public static IWindsorContainer AddAspectCoreFacility(this IWindsorContainer windsorContainer, Action<IAspectConfiguration> configure = null)
        {
            if (windsorContainer == null)
            {
                throw new ArgumentNullException(nameof(windsorContainer));
            }
            AddAspectCoreFacility(windsorContainer.Kernel, configure);
            return windsorContainer;
        }
    }
}