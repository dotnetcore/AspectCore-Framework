using System;
using System.Threading.Tasks;
#if NET45
using Nito.AsyncEx;
#endif

namespace AspectCore.Abstractions.Internal
{
    public class AspectActivator : IAspectActivator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAspectBuilderProvider aspectBuilderProvider;

        public AspectActivator(
            IServiceProvider serviceProvider, 
            IAspectBuilderProvider aspectBuilderProvider)
        {
            if (aspectBuilderProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectBuilderProvider));
            }
            this.serviceProvider = serviceProvider;
            this.aspectBuilderProvider = aspectBuilderProvider;
        }

        private async Task<T> ConvertReturnVaule<T>(object value)
        {
            if (value is Task<T>)
            {
                return await (Task<T>)value;
            }
            else if (value is Task)
            {
                await (Task)value;
                return default(T);
            }
            else
            {
                return (T)value;
            }
        }

        public T Invoke<T>(AspectActivatorContext activatorContext)
        {
            var invokeAsync = InvokeAsync<T>(activatorContext);

            if (invokeAsync.IsCompleted)
            {
                return invokeAsync.Result;
            }

#if NET45
            return AsyncContext.Run(() => invokeAsync);
#else

            return Task.Run(async () => await invokeAsync).GetAwaiter().GetResult();
#endif
        }

        public async Task<T> InvokeAsync<T>(AspectActivatorContext activatorContext)
        {
            using (var context = new DefaultAspectContext<T>(serviceProvider, activatorContext))
            {
                var aspectBuilder = aspectBuilderProvider.GetBuilder(activatorContext);

                await aspectBuilder.Build()(() => context.Target.Invoke(context.Parameters))(context);

                return await ConvertReturnVaule<T>(context.ReturnParameter.Value);
            }
        }
    }
}