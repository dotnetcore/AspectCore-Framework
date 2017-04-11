using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectActivator
    {
        T Invoke<T>(AspectActivatorContext activatorContext);

        Task<T> InvokeAsync<T>(AspectActivatorContext activatorContext);
    }
}
