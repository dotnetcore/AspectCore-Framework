using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface IPropertyInjectorSelector
    {
        IPropertyInjector[] SelectPropertyInjector(Type interceptorType);
    }
}
