using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal class AspectContextFactory: IAspectContextFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public AspectContext Create()
        {
            return new MethodAspectContext(null, null, null, null, _serviceProvider);
        }
    }
}
