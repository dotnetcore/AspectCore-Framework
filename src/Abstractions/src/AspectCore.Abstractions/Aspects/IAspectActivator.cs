using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectActivator
    {
        TResult Invoke<TResult>(AspectActivatorContext activatorContext);

        Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext);

        ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext);
    }
}