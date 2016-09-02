using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal class AspectServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider serviceProvider;

        public object GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
        }
    }
}
