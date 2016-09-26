using AspectCore.Lite.Core;
using System;
using AspectCore.Lite.Core.Descriptors;

namespace AspectCore.Lite.Internal
{
    internal sealed class InternalAspectContext : AspectContext
    {
        public InternalAspectContext(Target target, Proxy proxy, ParameterCollection parameters, ParameterDescriptor returnParameter) 
            : base(target, proxy, parameters, returnParameter)
        {
        }

        public override IServiceProvider ApplicationServices
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IServiceProvider AspectServices
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
