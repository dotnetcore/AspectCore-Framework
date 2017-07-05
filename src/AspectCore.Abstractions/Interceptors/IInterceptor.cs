using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptor
    {
        int Order { get; set; }

        bool AllowMultiple { get; }

        ExecutionMode Execution { get; set; }

        Task Invoke(AspectContext context, AspectDelegate next);
    }
}