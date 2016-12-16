using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectConfiguratorFactory<TContainer>
    {
        IAspectConfigurator CreateConfigurator(TContainer container);
    }
}
