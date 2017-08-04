using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IServiceDefinition
    {
        object Key { get; }

        Type ServiceType { get; }

        Lifetime Lifetime { get; }
    }
}
