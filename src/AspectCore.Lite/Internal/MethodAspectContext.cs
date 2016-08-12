
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.Core;

namespace AspectCore.Lite.Internal
{
    internal sealed class MethodAspectContext : AspectContext
    {
        internal MethodAspectContext(Target target, Proxy proxy, ParameterCollection parameters, ParameterDescriptor returnParameter, IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
            Proxy = proxy;
            Target = target;
            Parameters = parameters;
            ReturnParameter = returnParameter;
        }

        public override ParameterCollection Parameters { get; }

        public override Proxy Proxy { get; }

        public override ParameterDescriptor ReturnParameter { get; }

        public override Target Target { get; }
    }
}
