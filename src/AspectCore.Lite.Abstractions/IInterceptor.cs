using AspectCore.Lite.Abstractions.Attributes;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptor
    {
        int Order { get; set; }

        bool AllowMultiple { get; }

        Task Invoke(IAspectContext context, AspectDelegate next);
    }
}
