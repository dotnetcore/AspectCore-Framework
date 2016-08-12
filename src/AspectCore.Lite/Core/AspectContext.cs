using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public abstract class AspectContext : IDisposable
    {
        private readonly IServiceScope serviceScope;
        public IServiceProvider ApplicationServices { get; }
        public IServiceProvider AspectServices { get; }

        public virtual Target Target { get; }
        public virtual Proxy Proxy { get; }
        public virtual ParameterCollection Parameters { get; }
        public virtual ParameterDescriptor ReturnParameter { get; }

        protected internal AspectContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            ApplicationServices = serviceProvider;
            serviceScope = ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            AspectServices = serviceScope.ServiceProvider;
        }

        public virtual void Dispose()
        {
            serviceScope.Dispose();
        }
    }
}
