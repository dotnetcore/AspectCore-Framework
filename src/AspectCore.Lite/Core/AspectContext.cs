using AspectCore.Lite.Core.Descriptors;
using System;

namespace AspectCore.Lite.Core
{
    public abstract class AspectContext : IDisposable
    {
        public virtual IServiceProvider ApplicationServices { get; }
        public virtual IServiceProvider AspectServices { get; }
        public virtual Target Target { get; }
        public virtual Proxy Proxy { get; }
        public virtual ParameterCollection Parameters { get; }
        public virtual ParameterDescriptor ReturnParameter { get; }

        protected internal AspectContext(Target target, Proxy proxy, ParameterCollection parameters, ParameterDescriptor returnParameter)
        {
            Proxy = proxy;
            Target = target;
            Parameters = parameters;
            ReturnParameter = returnParameter;
            Proxy.InjectionParameters(Parameters);
            Target.InjectionParameters(Parameters);
        }

        public virtual void Dispose()
        {
        }
    }
}
