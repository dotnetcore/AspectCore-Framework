using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptor
    {
        int Order { get; set; }

        bool AllowMultiple { get; set; }

        Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next);
    }
}
