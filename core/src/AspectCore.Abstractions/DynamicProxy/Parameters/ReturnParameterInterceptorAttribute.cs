using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    [AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public abstract class ReturnParameterInterceptorAttribute : Attribute, IParameterInterceptor
    {
        public abstract Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next);
    }
}