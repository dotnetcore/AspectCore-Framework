using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IInterceptor
    {
        int Order { get; set; }
        Task ExecuteAsync(AspectContext aspectContext, InterceptorDelegate next);
    }
}
