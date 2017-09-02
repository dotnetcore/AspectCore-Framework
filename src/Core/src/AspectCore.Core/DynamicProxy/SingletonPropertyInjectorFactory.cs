using System;
using AspectCore.Injector;

namespace AspectCore.DynamicProxy
{
    public sealed class SingletonPropertyInjectorFactory : PropertyInjectorFactory
    {
        public SingletonPropertyInjectorFactory(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}