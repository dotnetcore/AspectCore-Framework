using AspectCore.Lite.Abstractions.Aspects;
using AspectCore.Lite.Abstractions.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Methods.Aspects
{
    internal class MethodAspectContextFactory : IAspectContextFactory
    {
        private readonly ITarget target;
        private readonly IProxy proxy;
        private readonly ParameterCollection parameters;
        private readonly ParameterDescriptor returnParameter;

        public MethodAspectContextFactory(ITarget target , IProxy proxy , ParameterCollection parameters , ParameterDescriptor returnParameter)
        {
            this.target = target;
            this.proxy = proxy;
            this.parameters = parameters;
            this.returnParameter = returnParameter;
        }
        public AspectContext Create()
        {
            return new MethodAspectContext(target , proxy , parameters , returnParameter); ;
        }
    }
}
