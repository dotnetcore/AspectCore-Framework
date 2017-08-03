using System;

namespace AspectCore.Extensions.IoC
{
    public interface IServiceResolver : IServiceProvider
    {
        object Resolve(Type serviceType, object key);
    }
}
