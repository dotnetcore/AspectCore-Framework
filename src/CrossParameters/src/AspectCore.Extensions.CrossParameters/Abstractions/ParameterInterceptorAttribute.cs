using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class ParameterInterceptorAttribute : Attribute, IParameterInterceptor
    {
        public virtual Task Invoke(IParameterDescriptor parameter, ParameterAspectContext context, ParameterAspectDelegate next)
        {
            return next(parameter, context);
        }
    }
}
