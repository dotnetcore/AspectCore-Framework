using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;

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

        public TReturn Invoke<TReturn>(AspectActivatorContext activatorContext)
        {
            var invokeAsync = InvokeAsync<TReturn>(activatorContext);

            if (invokeAsync.IsFaulted)
            {
                throw invokeAsync.Exception?.InnerException;
            }

            if (invokeAsync.IsCompleted)
            {
                return invokeAsync.Result;
            }

            return invokeAsync.GetAwaiter().GetResult();
        }

        public async Task<TReturn> InvokeAsync<TReturn>(AspectActivatorContext activatorContext)
        {
            using (var context = _aspectContextFactory.CreateContext<TReturn>(activatorContext))
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                await aspectBuilder.Build()(() => context.Target.Invoke(context.Parameters))(context);
                return await Unwrap(context.ReturnParameter.Value);
            }

            async Task<TReturn> Unwrap(object value)
            {
                if (value is Task<TReturn> resultTask)
                {
                    return await resultTask;
                }
                else if (value is Task task)
                {
                    await task;
                    return default(TReturn);
                }
                else if(value is ValueTask<TReturn> valueTask)
                {
                    return await valueTask;
                }
                else
                {
                    return (TReturn)value;
                }
            }
        }
    }
}
