using System.Threading.Tasks;

namespace AspectCore.Abstractions.Extensions
{
    internal static class TaskCache
    {
        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}
