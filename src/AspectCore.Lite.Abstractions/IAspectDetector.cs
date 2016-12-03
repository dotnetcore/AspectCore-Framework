using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectDetector
    {
        bool HasAspect(MethodInfo serviceMethod);
    }
}
