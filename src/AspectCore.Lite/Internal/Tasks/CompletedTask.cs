using System.Threading.Tasks;

namespace AspectCore.Lite.Internal.Tasks
{
    internal static class CompletedTask
    {
        public readonly static Task Default = CompletedTask<object>.Default;
    }

    internal static class CompletedTask<TResult>
    {
        static CompletedTask()
        {
            Default = Task.FromResult<TResult>(default(TResult));
        }

        public readonly static Task<TResult> Default;
    }
}
