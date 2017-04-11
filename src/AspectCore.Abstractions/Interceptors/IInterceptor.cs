using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptor
    {
        int Order { get; set; }

        bool AllowMultiple { get; }

        Task Invoke(AspectContext context, AspectDelegate next);
    }
}
