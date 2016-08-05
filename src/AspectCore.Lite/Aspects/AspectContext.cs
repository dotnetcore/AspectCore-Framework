using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Aspects
{
    public abstract class AspectContext : IDisposable
    {
        private readonly IServiceScope serviceScope;
        public ITarget Target { get; }
        public IProxy Proxy { get; }
        public IServiceProvider ApplicationServices { get; }
        public IServiceProvider AspectServices { get; }

        public AspectContext(ITarget target, IProxy proxy, IServiceProvider serviceProvider)
        {
            Target = target;
            Proxy = proxy;
            ApplicationServices = serviceProvider;
            serviceScope = ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            AspectServices = serviceScope.ServiceProvider;
        }
             
        public void Dispose()
        {
            serviceScope.Dispose();
        }
    }
}
