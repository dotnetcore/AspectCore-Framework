using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Tests.Fakes
{
    class ServiceProvider : IServiceProvider, IServiceResolver
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            return null;
        }

        public object Resolve(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
