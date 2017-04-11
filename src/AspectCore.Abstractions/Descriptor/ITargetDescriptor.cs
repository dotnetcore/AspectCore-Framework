using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    public interface ITargetDescriptor
    {
        object ImplementationInstance { get; }
        MethodInfo ServiceMethod { get; }
        MethodInfo ImplementationMethod { get; }
        Type ServiceType { get; }
        Type ImplementationType { get; }

        object Invoke(IEnumerable<IParameterDescriptor> parameterDescriptors);
    }
}
