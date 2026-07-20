using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Castle.DynamicProxy;

namespace AspectCore.Extensions.CastleCompat
{
    /// <summary>
    /// Adapts a Castle <see cref="Castle.DynamicProxy.IInterceptor"/> to run inside
    /// AspectCore's interception pipeline.
    /// 
    /// <para>
    /// This allows existing Castle-style interceptors to be used with AspectCore
    /// during a gradual migration. The adapter wraps the AspectCore
    /// <see cref="AspectContext"/> into a Castle-compatible <see cref="IInvocation"/>.
    /// </para>
    /// 
    /// <para><b>Limitations:</b></para>
    /// <list type="bullet">
    /// <item>ref/out/ref readonly return values are not supported through this adapter</item>
    /// <item>IAsyncEnumerable interception requires native AspectCore interceptors</item>
    /// <item>Castle's IChangeProxyTarget is not supported</item>
    /// <item>Async-aware interception requires using AspectCore's native model</item>
    /// </list>
    /// </summary>
    [NonAspect]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public class CastleInterceptorAdapter : AbstractInterceptorAttribute
    {
        private readonly Castle.DynamicProxy.IInterceptor _castleInterceptor;

        /// <summary>
        /// Creates a new adapter wrapping the specified Castle interceptor.
        /// </summary>
        /// <param name="castleInterceptor">The Castle interceptor to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="castleInterceptor"/> is null.</exception>
        public CastleInterceptorAdapter(Castle.DynamicProxy.IInterceptor castleInterceptor)
        {
            _castleInterceptor = castleInterceptor ?? throw new ArgumentNullException(nameof(castleInterceptor));
        }

        /// <inheritdoc />
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var invocation = new AspectContextInvocationAdapter(context, next);
            _castleInterceptor.Intercept(invocation);

            if (invocation.AsyncResult is Task task)
            {
                return task;
            }

            return Task.CompletedTask;
        }
    }
}
