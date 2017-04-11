using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    public interface IParameterCollection : IEnumerable<IParameterDescriptor>, IReadOnlyList<IParameterDescriptor>
    {
        IParameterDescriptor this[string name] { get; }
    }
}