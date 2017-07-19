using System.Threading.Tasks;

namespace AspectCore.Core.Internal
{
    internal static class TaskCache
    {
        internal static readonly Task CompletedTask = Task.FromResult(false);
    }
}
