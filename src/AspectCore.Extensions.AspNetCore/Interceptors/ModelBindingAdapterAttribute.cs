using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DataValidation;
using AspectCore.Extensions.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspectCore.Extensions.AspNetCore
{
    public class ModelBindingAdapterAttribute : AbstractInterceptorAttribute
    {
        public override int Order { get; set; } = -998;

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            var httpContext = context.GetHttpContext();
            if (httpContext != null)
            {
                var modelState = httpContext.Items["modelstate-aspectcore"] as ModelStateDictionary;
                if (modelState != null)
                {
                    var dataState = GetDataState(context.Implementation);
                    if (dataState != null)
                    {
                        foreach(var error in dataState.Errors)
                        {
                            modelState.AddModelError(error.Key, error.ErrorMessage);
                        }
                    }
                }
            }
        }

        private IDataState GetDataState(object implementation)
        {
            if (implementation is IDataStateProvider dataStateProvider)
            {
                return dataStateProvider.DataState;
            }
            else
            {
                var implementationTypeInfo = implementation.GetType().GetTypeInfo();
                var dataStateProperty = implementationTypeInfo.GetProperty("DataState");
                if (dataStateProperty != null && dataStateProperty.CanRead)
                {
                    return dataStateProperty.GetReflector().GetValue(implementation) as IDataState;
                }
                return null;
            }
        }
    }
}