using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IPointcut
    {
        bool IsMatch(MethodInfo method);
    }
}
