using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.Core.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection
{
    public sealed class ServiceAspectContext : AspectContext
    {
        private readonly IServiceScope serviceScope;
        public override IServiceProvider ApplicationServices { get; }
        public override IServiceProvider AspectServices { get; }
        public ServiceAspectContext(Target target, Proxy proxy, ParameterCollection parameters, ParameterDescriptor returnParameter,IServiceProvider serviceProvider) 
            : base(target, proxy, parameters, returnParameter)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            ApplicationServices = serviceProvider;
            serviceScope = ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            AspectServices = serviceScope.ServiceProvider;
        }

        public override void Dispose()
        {
            serviceScope.Dispose();
            base.Dispose();
        }
    }
}
