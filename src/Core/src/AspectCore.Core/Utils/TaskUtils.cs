using System.Threading.Tasks;

namespace AspectCore.Utils
{
    internal static class TaskUtils
    {
        internal static readonly Task CompletedTask = Task.FromResult(false);
    }

    internal static class TaskUtils<T>
    {
        internal static readonly Task<T> CompletedTask = Task.FromResult(default(T));
    }
}