using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectContextFactoryProvider : IAspectContextFactoryProvider
    {
        private readonly IServiceProvider serviceProvider;
        public AspectContextFactoryProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IAspectContextFactory ContextFactory
        {
            get
            {
                return new AspectContextFactory(serviceProvider);
            }
        }
    }
}
