using System.Reflection;

namespace AspectCore.Lite.Core
{
    public interface IPointcut
    {
        bool IsMatch(MethodInfo method);
    }
}
