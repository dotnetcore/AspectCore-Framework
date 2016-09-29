using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public delegate Task InterceptorDelegate(IAspectContext aspectContext);
}
