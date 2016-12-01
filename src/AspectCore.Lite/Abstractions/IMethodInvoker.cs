namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IMethodInvoker
    {
        object Invoke();
    }
}
