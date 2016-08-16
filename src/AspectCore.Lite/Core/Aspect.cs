using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public class Aspect
    {
        public Type InterceptorType { get; }
        public IInterceptor Interceptor { get; }
        public IPointcut Pointcut { get; }

        internal Aspect(Type interceptorType , IPointcut pointcut)
        {
            InterceptorType = interceptorType;
            Pointcut = pointcut;
        }

        internal Aspect(IInterceptor interceptor , IPointcut pointcut)
        {
            Interceptor = interceptor;
            Pointcut = pointcut;
        }
    }
}
