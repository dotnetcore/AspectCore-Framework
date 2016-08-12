using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectContextFactoryFactory : IAspectContextFactoryFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public AspectContextFactoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAspectContextFactory Create()
        {
            return new AspectContextFactory(_serviceProvider);
        }
    }
}
