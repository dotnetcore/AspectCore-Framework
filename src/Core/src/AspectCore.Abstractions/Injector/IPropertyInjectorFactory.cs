using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface IPropertyInjectorFactory
    {
        IPropertyInjector Create(Type implementationType);
    }
}