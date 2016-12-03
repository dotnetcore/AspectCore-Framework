using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internals
{
    internal sealed class JoinPoint : IJoinPoint
    {
        private readonly IList<Func<InterceptorDelegate, InterceptorDelegate>> delegates;

        public IMethodInvoker MethodInvoker { get; set; }

        public JoinPoint()
        {
            delegates = new List<Func<InterceptorDelegate, InterceptorDelegate>>();
        }

        public void AddInterceptor(Func<InterceptorDelegate, InterceptorDelegate> interceptorDelegate)
        {
            ExceptionHelper.ThrowArgumentNull(interceptorDelegate , nameof(interceptorDelegate));
            delegates.Add(interceptorDelegate);
        }

        public InterceptorDelegate Build()
        {
            ExceptionHelper.Throw<InvalidOperationException>(() => MethodInvoker == null , "Calling proxy method failed.Because instance of ProxyMethodInvoker is null.");
            InterceptorDelegate next = context =>
            {
                var result = MethodInvoker.Invoke();
                context.ReturnParameter.Value = result;
                return Task.FromResult(0);
            };

            foreach (var @delegate in delegates.Reverse())
            {
                next = @delegate(next);
                ExceptionHelper.Throw<InvalidOperationException>(() => next == null , "Invalid interceptorDelegate.");
            }

            return next;
        }
    }
}
