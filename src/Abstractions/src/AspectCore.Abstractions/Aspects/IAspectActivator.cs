using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectActivator
    {
        TReturn Invoke<TReturn>(AspectActivatorContext activatorContext);

        Task<TReturn> InvokeAsync<TReturn>(AspectActivatorContext activatorContext);
    }
}
