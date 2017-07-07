using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptor : IExecutableInterceptor, ISortableInterceptor
    {
        bool AllowMultiple { get; }

        Task Invoke(AspectContext context, AspectDelegate next);
    }
}