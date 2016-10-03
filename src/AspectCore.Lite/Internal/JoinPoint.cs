using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Internal.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (ProxyMethodInvoker == null)
            {
                throw new InvalidOperationException("Calling proxy method failed.Because instance of ProxyMethodInvoker is null.");
            }

            InterceptorDelegate next = context =>
            {
                var result = ProxyMethodInvoker.Invoke();
                context.ReturnParameter.Value = result;
                return CompletedTask.Default;
            };

            foreach (var @delegate in delegates.Reverse())
            {
                next = @delegate(next);
            }

            return next;
        }
    }
}
