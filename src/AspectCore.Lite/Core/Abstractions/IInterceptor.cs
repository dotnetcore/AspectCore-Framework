using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public interface IInterceptor
    {
        int Order { get; set; }

        Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next);
    }
}
