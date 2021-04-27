using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 数据校验拦截器特性
    /// </summary>
    public class DataValidationInterceptorAttribute : AbstractInterceptorAttribute
    {
        /// <summary>
        /// 提供一个布尔值。如果为 true，则该特性可多次使用, false（单用的）。
        /// </summary>
        public override bool AllowMultiple => false;

        /// <summary>
        /// 排序号,用以指定拦截顺序
        /// </summary>
        public override int Order { get; set; } = -999;

        /// <summary>
        /// 增强的具体业务逻辑
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="next">后续处理拦截上下文的委托对象</param>
        /// <returns>异步任务</returns>
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