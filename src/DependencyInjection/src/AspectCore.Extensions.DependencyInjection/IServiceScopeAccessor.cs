using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public interface IServiceScopeAccessor
    {
        IServiceScope CurrentServiceScope { get; set; }
    }
}
