using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectActivator : IAspectActivator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAspectBuilderProvider _aspectBuilderProvider;
        private readonly IAspectContextScheduler _aspectContextScheduler;

        public AspectActivator(
            IServiceProvider serviceProvider,
            IAspectBuilderProvider aspectBuilderProvider,
           IAspectContextScheduler aspectContextScheduler)
        {
            if (aspectBuilderProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectBuilderProvider));
            }
            _serviceProvider = serviceProvider;
            _aspectBuilderProvider = aspectBuilderProvider;
            _aspectContextScheduler = aspectContextScheduler;
        }

        public T Invoke<T>(AspectActivatorContext activatorContext)
        {
            var invokeAsync = InvokeAsync<T>(activatorContext);

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

        public async Task<T> InvokeAsync<T>(AspectActivatorContext activatorContext)
        {
            var target = new TargetDescriptor(activatorContext.TargetInstance,
                activatorContext.ServiceMethod,
                activatorContext.ServiceType,
                activatorContext.TargetMethod,
                activatorContext.TargetInstance?.GetType() ?? activatorContext.TargetMethod.DeclaringType);

            var proxy = new ProxyDescriptor(activatorContext.ProxyInstance,
                activatorContext.ProxyMethod,
                activatorContext.ProxyInstance.GetType());

            var parameters = new ParameterCollection(activatorContext.Parameters,
                activatorContext.ServiceMethod.GetParameters());

            var returnParameter = new ReturnParameterDescriptor(default(T),
                activatorContext.ServiceMethod.ReturnParameter);

            var rtContext = new RuntimeAspectContext(_serviceProvider, target, proxy, parameters, returnParameter);
            using (var context = new ScopedAspectContext(rtContext, _aspectContextScheduler))
            {
                var aspectBuilder = _aspectBuilderProvider.GetBuilder(context);
                await aspectBuilder.Build()(() => context.Target.Invoke(context.Parameters))(context);
                return await Unwrap(context.ReturnParameter.Value);
            }

            async Task<T> Unwrap(object value)
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
        }
    }
}
