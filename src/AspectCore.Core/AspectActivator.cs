using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public class AspectActivator : IAspectActivator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAspectBuilderProvider _aspectBuilderProvider;

        public AspectActivator(
            IServiceProvider serviceProvider,
            IAspectBuilderProvider aspectBuilderProvider)
        {
            if (aspectBuilderProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectBuilderProvider));
            }
            _serviceProvider = serviceProvider;
            _aspectBuilderProvider = aspectBuilderProvider;
        }

        public T Invoke<T>(AspectActivatorContext activatorContext)
        {
            var invokeAsync = InvokeAsync<T>(activatorContext);

            if (invokeAsync.IsCompleted)
            {
                return invokeAsync.Result;
            }

            return Task.Run(async () => await invokeAsync).GetAwaiter().GetResult();
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

            using (var context = new DefaultAspectContext(_serviceProvider, target, proxy, parameters, returnParameter))
            {
                var aspectBuilder = _aspectBuilderProvider.GetBuilder(activatorContext);

                await aspectBuilder.Build()(() => context.Target.Invoke(context.Parameters))(context);

                return await ConvertReturnVaule<T>(context.ReturnParameter.Value);
            }
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

    }
}