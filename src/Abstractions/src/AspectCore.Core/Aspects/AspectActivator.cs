using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectActivator : IAspectActivator
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;

        public AspectActivator(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory)
        {
            _aspectContextFactory = aspectContextFactory ?? throw new ArgumentNullException(nameof(aspectContextFactory));
            _aspectBuilderFactory = aspectBuilderFactory ?? throw new ArgumentNullException(nameof(aspectBuilderFactory));
        }

        public TResult Invoke<TResult>(AspectActivatorContext activatorContext)
        {
            return (TResult)InternalInvoke<TResult>(activatorContext);
        }

        public Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext)
        {
            var result = InternalInvoke<TResult>(activatorContext);
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

        public ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext)
        {
            return (ValueTask<TResult>)InternalInvoke<TResult>(activatorContext);
        }

        private object InternalInvoke<TResult>(AspectActivatorContext activatorContext)
        {
            using (var context = _aspectContextFactory.CreateContext<TResult>(activatorContext))
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(() => context.Target.Invoke(context.Parameters))(context);
                if (invoke.IsFaulted)
                {
                    throw invoke.Exception?.InnerException;
                }
                if (!invoke.IsCompleted)
                {
                    invoke.GetAwaiter().GetResult();
                }
                return context.ReturnParameter.Value;
            }
        }
    }
}
