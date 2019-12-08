using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    public class EnableParameterAspectInterceptor : AbstractInterceptorAttribute
    {
        public override int Order { get; set; } = -9;

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