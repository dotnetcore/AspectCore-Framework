using AspectCore.Lite.Core.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public sealed class Aspect
    {
        public IInterceptor Interceptor { get; }
        public IAsyncInterceptor AsyncInterceptor { get; }
        public IPointcut Pointcut { get; }

        private Aspect(IInterceptor interceptor, IAsyncInterceptor asyncInterceptor, IPointcut pointcut)
        {
            Interceptor = interceptor;
            AsyncInterceptor = asyncInterceptor;
            Pointcut = pointcut;
        }

        public static Aspect Create(IInterceptor interceptor, IPointcut pointcut)
        {
            if (interceptor == null) throw new ArgumentNullException(nameof(interceptor));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            return new Aspect(interceptor, null, pointcut);
        }

        public static Aspect Create(IAsyncInterceptor asyncInterceptor, IPointcut pointcut)
        {
            if (asyncInterceptor == null) throw new ArgumentNullException(nameof(asyncInterceptor));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            return new Aspect(null, asyncInterceptor, pointcut);
        }
    }
}
