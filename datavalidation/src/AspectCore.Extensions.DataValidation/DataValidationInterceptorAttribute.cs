using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.DataValidation
{
    public class DataValidationInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override bool AllowMultiple => false;

        public override int Order { get; set; } = -999;

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var dataValidator = context.ServiceProvider.GetService(typeof(IDataValidator)) as IDataValidator;
            CheckResolved(dataValidator);
            var dataStateFactory = context.ServiceProvider.GetService(typeof(IDataStateFactory)) as IDataStateFactory;
            CheckResolved(dataStateFactory);
            var dataValidationContext = new DataValidationContext(context);
            dataValidator.Validate(dataValidationContext);
            context.SetDataValidationContext(dataValidationContext);
            var dataState = dataStateFactory.CreateDataState(dataValidationContext);
            if (context.Implementation is IDataStateProvider dataStateProvider)
            {
                dataStateProvider.DataState = dataState;
            }
            else
            {
                var implementationTypeInfo = context.Implementation.GetType().GetTypeInfo();
                var dataStateProperty = implementationTypeInfo.GetProperty("DataState");
                if (dataStateProperty != null && dataStateProperty.CanWrite)
                {
                    dataStateProperty.GetReflector().SetValue(context.Implementation, dataState);
                }
            }
            return context.Invoke(next);
        }

        private void CheckResolved<T>(T service)
        {
            if (service == null)
                throw new InvalidOperationException($"No instance for type '{typeof(T)}' has been resolved.");
        }
    }
}