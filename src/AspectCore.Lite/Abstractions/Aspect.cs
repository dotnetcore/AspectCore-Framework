using System;

namespace AspectCore.Lite.Abstractions
{
    public sealed class Aspect
    {
        public IInterceptor Interceptor { get; }
        public IPointcut Pointcut { get; }

        private Aspect(IInterceptor interceptor, IPointcut pointcut)
        {
            Interceptor = interceptor;
            Pointcut = pointcut;
        }

        public static Aspect Create(IInterceptor interceptor, IPointcut pointcut)
        {
            if (interceptor == null) throw new ArgumentNullException(nameof(interceptor));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            return new Aspect(interceptor, pointcut);
        }
    }
}
