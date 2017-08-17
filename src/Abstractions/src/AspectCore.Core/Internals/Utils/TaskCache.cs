using System.Threading.Tasks;

namespace AspectCore.Core.Internal
{
    internal static class TaskCache
    {
        internal static readonly Task CompletedTask = Task.FromResult(false);
    }

    internal static class TaskCache<T>
    {
        internal static readonly Task<T> CompletedTask = Task.FromResult(default(T));
    }
}