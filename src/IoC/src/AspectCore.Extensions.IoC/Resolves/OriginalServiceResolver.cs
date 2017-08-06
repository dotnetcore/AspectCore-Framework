using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal class OriginalServiceResolver : IOriginalServiceResolver
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type serviceType, object key)
        {
            throw new NotImplementedException();
        }
    }
}