using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public interface IAspectCoreBuilder
    {
        IServiceCollection Services { get; }
    }
}