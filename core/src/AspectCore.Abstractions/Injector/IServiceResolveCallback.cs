using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect, NonCallback]
    public interface IServiceResolveCallback
    {
        object Invoke(IServiceResolver resolver, object instance, ServiceDefinition service);
    }

    public sealed class NonCallback : Attribute
    {
    }
}