using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal class TargetServiceResolver : ITargetServiceResolver
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type serviceType, string key)
        {
            throw new NotImplementedException();
        }
    }
}