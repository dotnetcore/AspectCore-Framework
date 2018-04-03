 using System;
using System.Threading.Tasks;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    internal sealed class AspectActivator : IAspectActivator
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;

        public AspectActivator(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory)
        {
            _aspectContextFactory = aspectContextFactory;
            _aspectBuilderFactory = aspectBuilderFactory;
        }

        public TResult Invoke<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);
                if (invoke.IsFaulted)
                {
                    var innerException = invoke.Exception?.InnerException;
                    throw context.InvocationException(innerException);
                }
            
                return (TResult)context.ReturnValue;
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }

        public Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);
                if (invoke.IsFaulted)
                {
                    var innerException = invoke.Exception?.InnerException;
                    throw context.InvocationException(innerException);
                }
              
                var result = context.ReturnValue;
                if (result == null)
                {
                    return default(Task<TResult>);
                }
                else if (result is Task<TResult> resultTask)
                {
                    return resultTask;
                }
                else if (result is Task task)
                {
                    return TaskUtils<TResult>.CompletedTask;
                }
                else
                {
                    throw context.InvocationException(new InvalidCastException($"Unable to cast object of type '{result.GetType()}' to type '{typeof(Task<TResult>)}'."));
                }
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }

        public ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);
                if (invoke.IsFaulted)
                {
                    var innerException = invoke.Exception?.InnerException;
                    throw context.InvocationException(innerException);
                }
              
                return (ValueTask<TResult>)context.ReturnValue;
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }
    }
}