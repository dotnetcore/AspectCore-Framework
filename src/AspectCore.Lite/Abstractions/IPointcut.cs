using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public interface IPointcut
    {
        bool IsMatch(MethodInfo method);
    }
}
