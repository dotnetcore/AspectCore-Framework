using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
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
            using (var context = _aspectContextFactory.CreateContext(activatorContext))
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);
                if (invoke.IsFaulted)
                {
                    throw invoke.Exception?.InnerException;
                }
                if (!invoke.IsCompleted)
                {
                    invoke.GetAwaiter().GetResult();
                }
                return (TResult)context.ReturnValue;
            }
        }

        public Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext)
        {
            using (var context = _aspectContextFactory.CreateContext(activatorContext))
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);
                if (invoke.IsFaulted)
                {
                    throw invoke.Exception?.InnerException;
                }
                if (!invoke.IsCompleted)
                {
                    invoke.GetAwaiter().GetResult();
                }
                var result = context.ReturnValue;
                if (result is Task<TResult> resultTask)
                {
                    return resultTask;
                }
                else if (result is Task task)
                {
                    if (!task.IsCompleted)
                    {
                        task.GetAwaiter().GetResult();
                    }
                    return TaskCache<TResult>.CompletedTask;
                }
                else
                {
                    throw new InvalidCastException($"Unable to cast object of type '{result.GetType()}' to type '{typeof(Task<TResult>)}'.");
                }
            }   
        }

        public ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext)
        {
            using (var context = _aspectContextFactory.CreateContext(activatorContext))
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);
                if (invoke.IsFaulted)
                {
                    throw invoke.Exception?.InnerException;
                }
                if (!invoke.IsCompleted)
                {
                    invoke.GetAwaiter().GetResult();
                }
                return (ValueTask<TResult>)context.ReturnValue;
            }
        }
    }
}