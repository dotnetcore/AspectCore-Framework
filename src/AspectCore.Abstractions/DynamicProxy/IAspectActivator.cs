using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectActivator
    {
        TResult Invoke<TResult>(AspectActivatorContext activatorContext);

        Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext);

        ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext);

        IAsyncEnumerable<TResult> InvokeAsyncEnumerable<TResult>(AspectActivatorContext activatorContext);
    }
}