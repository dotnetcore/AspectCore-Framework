using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public abstract class ParameterInterceptorAttribute : Attribute, IParameterInterceptor
    {
        public abstract Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next);
    }
}