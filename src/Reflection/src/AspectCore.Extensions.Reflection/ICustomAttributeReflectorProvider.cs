using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    public interface ICustomAttributeReflectorProvider
    {
        IEnumerable<CustomAttributeReflector> CustomAttributeReflectors { get; }
    }
}
