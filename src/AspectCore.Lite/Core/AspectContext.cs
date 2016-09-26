using AspectCore.Lite.Core.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using System;

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

        protected internal AspectContext(Target target, Proxy proxy, ParameterCollection parameters, ParameterDescriptor returnParameter, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            ApplicationServices = serviceProvider;
            serviceScope = ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            AspectServices = serviceScope.ServiceProvider;

            Proxy = proxy;
            Target = target;
            Parameters = parameters;
            ReturnParameter = returnParameter;

            Proxy.InjectionParameters(Parameters);
            Target.InjectionParameters(Parameters);
        }

        public virtual void Dispose()
        {
            serviceScope.Dispose();
        }
    }
}
