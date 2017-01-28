using System;

namespace AspectCore.Abstractions.Resolution.Internal
{
    [NonAspect]
    public interface IPropertyInjectorSelector
    {
        IPropertyInjector[] SelectPropertyInjector(Type interceptorType);
    }
}
