using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters
{
    [NonAspect]
    public interface IParameterInterceptor
    {
        Task Invoke(IParameterDescriptor parameter, ParameterAspectContext context, ParameterAspectDelegate next);
    }
}
