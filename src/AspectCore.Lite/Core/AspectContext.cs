using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public class AspectContext : IDisposable
    {
        private readonly IServiceScope serviceScope;
        public IServiceProvider ApplicationServices { get; }
        public IServiceProvider AspectServices { get; }

        public Target Target { get; }
        public Proxy Proxy { get; }
        public ParameterCollection Parameters { get; }
        public ParameterDescriptor ReturnParameter { get; }

        protected internal AspectContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            ApplicationServices = serviceProvider;
            serviceScope = ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            AspectServices = serviceScope.ServiceProvider;
        }

        protected internal AspectContext(Target target, Proxy proxy, ParameterCollection parameters, ParameterDescriptor returnParameter, IServiceProvider serviceProvider)
            : this(serviceProvider)
        {
            Proxy = proxy;
            Target = target;
            Parameters = parameters;
            ReturnParameter = returnParameter;
        }

        public virtual void Dispose()
        {
            serviceScope.Dispose();
        }
    }
}
