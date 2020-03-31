using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IInterceptor
    {
        bool AllowMultiple { get; }

        bool Inherited { get; set; }

        int Order { get; set; }

        Task Invoke(AspectContext context, AspectDelegate next);
    }
}
