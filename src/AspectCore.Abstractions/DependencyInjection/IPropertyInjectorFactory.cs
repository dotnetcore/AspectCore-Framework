using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface IPropertyInjectorFactory
    {
        IPropertyInjector Create(Type implementationType);
    }
}