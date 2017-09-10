using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    [NonAspect]
    public interface IParameterInterceptor
    {
        Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next);
    }
}