using System;
using AspectCore.Configuration;
using Castle.MicroKernel;
using Castle.Windsor;
using System.Linq;

namespace AspectCore.Extensions.Windsor
{
    public static class FacilityExtensions
    {
        public static IKernel AddAspectCoreFacility(this IKernel Kernel, Action<IAspectConfiguration> configure = null)
        {
            if (Kernel == null)
            {
                throw new ArgumentNullException(nameof(Kernel));
            }
            var config = new AspectConfiguration();
            configure?.Invoke(config);
            if (Kernel.GetFacilities().All(x => !(x.GetType() == typeof(AspectCoreFacility))))
                Kernel.AddFacility(new AspectCoreFacility(config));
            return Kernel;
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