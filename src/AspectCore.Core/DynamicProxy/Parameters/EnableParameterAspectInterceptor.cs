using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 针对被代理方法参数进行拦截处理的拦截器特性
    /// </summary>
    public class EnableParameterAspectInterceptor : AbstractInterceptorAttribute
    {
        public override int Order { get; set; } = -9;

        /// <summary>
        /// 拦截被代理方法参数执行处理逻辑
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="next">拦截委托</param>
        /// <returns>异步任务</returns>
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var selector = (IParameterInterceptorSelector)context.ServiceProvider.GetService(typeof(IParameterInterceptorSelector));
            if (selector == null)
            {
                throw new InvalidOperationException("Cannot resolve ParameterInterceptorSelector.");
            }
            var parameters = context.GetParameters();
            var count = parameters.Count;
            if (count > 0)
            {
                var parameterAspectInvoker = new ParameterAspectInvoker();
                for (var i = 0; i < count; i++)
                {
                    var parameter = parameters[i];
                    //获取对应参数的参数拦截器
                    var interceptors = selector.Select(parameter.ParameterInfo);
                    if (interceptors.Length > 0)
                    {
                        var parameterAspectContext = new ParameterAspectContext(context, parameter);
                        foreach (var interceptor in interceptors)
                        {
                            parameterAspectInvoker.AddDelegate(interceptor.Invoke);
                        }
                        await parameterAspectInvoker.Invoke(parameterAspectContext);
                        parameterAspectInvoker.Reset();
                    }
                }
            }
            await next(context);
            var returnParameter = context.GetReturnParameter();

            //返回值的拦截处理
            var returnInterceptors = selector.Select(returnParameter.ParameterInfo);
            if (returnInterceptors.Length > 0)
            {
                var returnParameterAspectContext = new ParameterAspectContext(context, returnParameter);
                var returnParameterAspectInvoker = new ParameterAspectInvoker();
                foreach (var interceptor in returnInterceptors)
                {
                    returnParameterAspectInvoker.AddDelegate(interceptor.Invoke);
                }
                await returnParameterAspectInvoker.Invoke(returnParameterAspectContext);
            }
        }
    }
}