using System;
#if NET8_0_OR_GREATER
using Microsoft.Extensions.ObjectPool;
#endif

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectContextFactory : IAspectContextFactory
    {
        private static readonly object[] emptyParameters = new object[0];
        private readonly IServiceProvider _serviceProvider;

#if NET8_0_OR_GREATER
        private static readonly ObjectPool<SourceGeneratedAspectContext> _sgContextPool =
            new DefaultObjectPoolProvider { MaximumRetained = Environment.ProcessorCount * 2 }
                .Create(new SgContextPoolPolicy());

        private sealed class SgContextPoolPolicy : IPooledObjectPolicy<SourceGeneratedAspectContext>
        {
            public SourceGeneratedAspectContext Create() => new SourceGeneratedAspectContext();
            public bool Return(SourceGeneratedAspectContext obj) => true;
        }
#endif

        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            return new RuntimeAspectContext(_serviceProvider,
                activatorContext.ServiceMethod,
                activatorContext.TargetMethod,
                activatorContext.ProxyMethod,
                activatorContext.PredicateMethod,
                activatorContext.TargetInstance,
                activatorContext.ProxyInstance,
                activatorContext.Parameters ?? emptyParameters);
        }

        public AspectContext CreateContext(AspectActivatorContext activatorContext, IAspectInvokeDelegate invokeDelegate)
        {
#if NET8_0_OR_GREATER
            var context = _sgContextPool.Get();
            context.Reset(
                _serviceProvider,
                activatorContext.ServiceMethod,
                activatorContext.TargetMethod,
                activatorContext.ProxyMethod,
                activatorContext.PredicateMethod,
                activatorContext.TargetInstance,
                activatorContext.ProxyInstance,
                activatorContext.Parameters ?? emptyParameters,
                invokeDelegate);
            return context;
#else
            return new SourceGeneratedAspectContext(_serviceProvider,
                activatorContext.ServiceMethod,
                activatorContext.TargetMethod,
                activatorContext.ProxyMethod,
                activatorContext.PredicateMethod,
                activatorContext.TargetInstance,
                activatorContext.ProxyInstance,
                activatorContext.Parameters ?? emptyParameters,
                invokeDelegate);
#endif
        }

        public void ReleaseContext(AspectContext aspectContext)
        {
#if NET8_0_OR_GREATER
            if (aspectContext is SourceGeneratedAspectContext sgCtx)
            {
                sgCtx.Clear();
                _sgContextPool.Return(sgCtx);
                return;
            }
#endif
            (aspectContext as IDisposable)?.Dispose();
        }
    }
}
