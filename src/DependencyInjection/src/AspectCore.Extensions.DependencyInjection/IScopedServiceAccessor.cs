using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Extensions.DependencyInjection
{
    public interface IScopedServiceAccessor<out T>
    {
        T Value { get; }

        T RequiredValue { get; }
    }
}
