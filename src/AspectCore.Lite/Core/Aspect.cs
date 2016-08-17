using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public sealed class Aspect
    {
        //public Type InterceptorType { get; }
        public IInterceptor Interceptor { get; }
        public IAsyncInterceptor AsyncInterceptor { get; }
        public IPointcut Pointcut { get; }

        //internal Aspect(Type interceptorType , IPointcut pointcut)
        //{
        //    InterceptorType = interceptorType;
        //    Pointcut = pointcut;
        //}
        private Aspect(IInterceptor interceptor, IAsyncInterceptor asyncInterceptor, IPointcut pointcut)
        {
            Interceptor = interceptor;
            AsyncInterceptor = asyncInterceptor;
            Pointcut = pointcut;
        }

        public static Aspect Sync(IInterceptor interceptor, IPointcut pointcut)
        {
            if (interceptor == null) throw new ArgumentNullException(nameof(interceptor));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            return new Aspect(interceptor, null, pointcut);
        }

        public static Aspect Async(IAsyncInterceptor asyncInterceptor, IPointcut pointcut)
        {
            if (asyncInterceptor == null) throw new ArgumentNullException(nameof(asyncInterceptor));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            return new Aspect(null, asyncInterceptor, pointcut);
        }

        //internal Aspect(IAsyncInterceptor asyncInterceptor, IPointcut pointcut)
        //{
        //    AsyncInterceptor = asyncInterceptor;
        //    Pointcut = pointcut;
        //}

        //internal Aspect(IInterceptor interceptor, IPointcut pointcut)
        //{
        //    Interceptor = interceptor;
        //    Pointcut = pointcut;
        //}
    }
}
