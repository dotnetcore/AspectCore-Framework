using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public class Aspect
    {
        public Type AdviceType { get; }
        public IAdvice Advice { get; }
        public IPointcut Pointcut { get; }

        internal Aspect(Type adviceType, IPointcut pointcut)
        {
            AdviceType = adviceType;
            Pointcut = pointcut;
        }

        internal Aspect(IAdvice advice, IPointcut pointcut)
        {
            Advice = advice;
            Pointcut = pointcut;
        }
    }
}
