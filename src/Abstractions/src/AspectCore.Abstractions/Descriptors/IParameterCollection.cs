using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IParameterCollection : IEnumerable<IParameterDescriptor>, IReadOnlyList<IParameterDescriptor>
    {
        IParameterDescriptor this[string name] { get; }
    }
}