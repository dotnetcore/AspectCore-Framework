using System;

namespace AspectCore.Injector
{
    public interface IPropertyInjectorFactory
    {
        IPropertyInjector Create(Type implementationType);
    }
}