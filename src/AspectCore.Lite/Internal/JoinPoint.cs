using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal class JoinPoint : IJoinPoint
    {
        private readonly IList<Func<InterceptorDelegate, InterceptorDelegate>> delegates;

        public IMethodInvoker ProxyMethodInvoker { get; set; }

        public JoinPoint()
        {
            delegates = new List<Func<InterceptorDelegate, InterceptorDelegate>>();
        }

        public void AddInterceptor(Func<InterceptorDelegate, InterceptorDelegate> interceptorDelegate)
        {
            if (interceptorDelegate == null)
            {
                throw new ArgumentNullException(nameof(interceptorDelegate));
            }

            delegates.Add(interceptorDelegate);
        }

        public InterceptorDelegate Build()
        {
            InterceptorDelegate next = context =>
            {
                var result = ProxyMethodInvoker.Invoke();
                context.ReturnParameter.Value = result;
                return Task.FromResult(0);
            };

            foreach (var @delegate in delegates.Reverse())
            {
                next = @delegate(next);
            }

            return next;
        }
    }
}
