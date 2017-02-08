using System;

namespace AspectCore.Abstractions.Internal
{
    [NonAspect]
    public interface IPropertyInjectorSelector
    {
        IPropertyInjector[] SelectPropertyInjector(Type interceptorType);
    }
}
