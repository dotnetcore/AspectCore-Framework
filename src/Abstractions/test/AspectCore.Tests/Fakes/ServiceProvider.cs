using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Tests.Fakes
{
    class ServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return null;
        }
    }
}
