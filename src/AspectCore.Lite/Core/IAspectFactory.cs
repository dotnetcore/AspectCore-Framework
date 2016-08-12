using AspectCore.Lite;
using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IAspectFactory
    {
        Aspect Create(IAdvice advice, IPointcut pointcut);

        Aspect Create(Type adviceType, IPointcut pointcut);
    }
}
