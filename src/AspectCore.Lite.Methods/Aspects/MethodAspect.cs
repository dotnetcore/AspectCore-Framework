using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Methods.Aspects
{
    public class MethodAspect : IAspect
    {
        public Func<IServiceProvider , IAdvice> AdviceFactory { get; set; }

        public IPointcut Pointcut { get; set; }

        public ITarget Target { get; set; }

        public IProxy Proxy { get; set; }

        public AspectDelegate CreateDelegate()
        {
            throw new NotImplementedException();
        }
    }
}
