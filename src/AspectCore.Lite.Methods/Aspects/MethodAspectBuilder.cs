using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Methods.Aspects
{
    public class MethodAspectBuilder : IAspectFactory
    {
         

        public IAspect Build()
        {
            throw new NotImplementedException();
        }

        public IAspect Create(object advice, IPointcut pointcut)
        {
            throw new NotImplementedException();
        }
    }
}
