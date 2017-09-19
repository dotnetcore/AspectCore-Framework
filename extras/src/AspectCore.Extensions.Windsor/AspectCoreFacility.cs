using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Configuration;
using Castle.Core;
using Castle.Core.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;

namespace AspectCore.Extensions.Windsor
{
    public class AspectCoreFacility : IFacility
    {
        private IKernel _kernel;

        private readonly IAspectConfiguration _aspectConfiguration;
        private readonly IEnumerable<ServiceDefinition> _services;

        public AspectCoreFacility(IAspectConfiguration aspectConfiguration, ServiceDefinition services)
        {
            _kernel.BeginScope();
        }

        public void Init(IKernel kernel, IConfiguration facilityConfig)
        {
            _kernel = kernel;

            kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
        }

        private void Kernel_ComponentModelCreated(ComponentModel model)
        {
            model.Interceptors.AddIfNotInCollection(InterceptorReference.ForType<AspectCoreInterceptor>());
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