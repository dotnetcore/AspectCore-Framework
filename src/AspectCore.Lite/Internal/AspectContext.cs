using AspectCore.Lite.Abstractions;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectContext : IAspectContext
    {
        private readonly IServiceScope serviceScope;
        public IServiceProvider ApplicationServices { get; }
        public IServiceProvider AspectServices { get; }
        public ParameterCollection Parameters { get; set; }
        public Proxy Proxy { get; set; }
        public ParameterDescriptor ReturnParameter { get; set; }
        public Target Target { get; set; }

        private bool dispose = false;

        internal AspectContext(IServiceProvider serviceProvider)
             : base()
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            ApplicationServices = serviceProvider;
            serviceScope = ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            AspectServices = serviceScope.ServiceProvider;
        }

        public void Dispose()
        {
            if (!dispose)
            {
                serviceScope.Dispose();
                dispose = true;
            }
        }
    }
}