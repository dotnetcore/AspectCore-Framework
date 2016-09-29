using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public delegate Task InterceptorDelegate(IAspectContext aspectContext);
}
