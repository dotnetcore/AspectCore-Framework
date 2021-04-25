using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DataValidation;
using AspectCore.Extensions.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspectCore.Extensions.AspNetCore
{
    /// <summary>
    /// 模型绑定适配特性
    /// </summary>
    public class ModelBindingAdapterAttribute : AbstractInterceptorAttribute
    {
        /// <summary>
        /// 排序号,用以指定拦截顺序
        /// </summary>
        public override int Order { get; set; } = -998;

        /// <summary>
        /// 增强的具体业务逻辑
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="next">后续处理拦截上下文的委托对象</param>
        /// <returns>异步任务</returns>
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

        /// <summary>
        /// 获取数据校验状态
        /// </summary>
        /// <param name="implementation">对象</param>
        /// <returns>数据校验状态</returns>
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