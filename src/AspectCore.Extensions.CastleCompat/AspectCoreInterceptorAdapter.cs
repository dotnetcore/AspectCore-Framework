using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Castle.DynamicProxy;

namespace AspectCore.Extensions.CastleCompat
{
    /// <summary>
    /// Adapts an AspectCore interceptor to work as a Castle <see cref="Castle.DynamicProxy.IInterceptor"/>.
    /// 
    /// <para>
    /// Use this when you need to register AspectCore-style interceptors in a Castle/Windsor container
    /// during a migration period where both frameworks coexist.
    /// </para>
    /// </summary>
    public sealed class AspectCoreInterceptorAdapter : Castle.DynamicProxy.IInterceptor
    {
        private readonly AbstractInterceptorAttribute _aspectCoreInterceptor;

        /// <summary>
        /// Creates a new adapter wrapping the specified AspectCore interceptor.
        /// </summary>
        /// <param name="aspectCoreInterceptor">The AspectCore interceptor to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aspectCoreInterceptor"/> is null.</exception>
        public AspectCoreInterceptorAdapter(AbstractInterceptorAttribute aspectCoreInterceptor)
        {
            _aspectCoreInterceptor = aspectCoreInterceptor ?? throw new ArgumentNullException(nameof(aspectCoreInterceptor));
        }

        /// <inheritdoc />
        public void Intercept(IInvocation invocation)
        {
            var context = new InvocationAspectContext(invocation);
            var task = _aspectCoreInterceptor.Invoke(context, InvocationAspectContext.CreateProceedDelegate(invocation));

            if (!task.IsCompleted)
            {
                // Block on the async result in Castle's sync model.
                // This is a necessary limitation of adapting async -> sync.
                task.GetAwaiter().GetResult();
            }
            else
            {
                task.GetAwaiter().GetResult(); // propagate exceptions
            }

            if (context.ReturnValue != null)
            {
                invocation.ReturnValue = context.ReturnValue;
            }
        }
    }
}
