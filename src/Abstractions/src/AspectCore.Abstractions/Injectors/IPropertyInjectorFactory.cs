using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IPropertyInjectorFactory
    {
        IPropertyInjector Create(Type implementationType);
    }
}