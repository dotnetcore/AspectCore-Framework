using System.Threading.Tasks;

namespace AspectCore.Abstractions.Internal
{
    internal static class TaskCache
    {
        internal static readonly Task CompletedTask = Task.FromResult(false);
    }
}
