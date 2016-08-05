using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Implementation
{
    public class AspectFactory : IAspectFactory
    {
        public IAspect Create(IAdvice advice, IPointcut pointcut)
        {
            throw new NotImplementedException();
        }
    }
}
