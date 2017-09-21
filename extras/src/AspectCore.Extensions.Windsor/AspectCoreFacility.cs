using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.MicroKernel;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    public class AspectCoreFacility : IFacility
    {
        private IKernel _kernel;

        private readonly IAspectConfiguration _aspectConfiguration;
        private readonly IEnumerable<ServiceDefinition> _services;

        public AspectCoreFacility(IAspectConfiguration aspectConfiguration, IEnumerable<ServiceDefinition> services)
        {
            _services = services;
            _aspectConfiguration = aspectConfiguration ?? new AspectConfiguration();
        }

        public void Init(IKernel kernel, IConfiguration facilityConfig)
        {
            _kernel = kernel;
            kernel.ComponentModelCreated += Kernel_ComponentModelCreated;

        }

        private void Kernel_ComponentModelCreated(ComponentModel model)
        {
            var aspectValidator = new AspectValidatorBuilder(_aspectConfiguration).Build();
            if (aspectValidator.Validate(model.Implementation) || model.Services.Any(x => aspectValidator.Validate(x)))
            {
                model.Interceptors.AddIfNotInCollection(InterceptorReference.ForType<AspectCoreInterceptor>());
            }
        }

        public void Terminate()
        {
            if (_kernel != null)
            {
                _kernel.ComponentModelCreated -= Kernel_ComponentModelCreated;
            }
        }
    }
}