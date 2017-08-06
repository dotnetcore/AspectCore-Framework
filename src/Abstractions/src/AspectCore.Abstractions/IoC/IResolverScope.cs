using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IResolverScope : IDisposable
    {
        IServiceResolver ServiceResolver { get; }
    }
}