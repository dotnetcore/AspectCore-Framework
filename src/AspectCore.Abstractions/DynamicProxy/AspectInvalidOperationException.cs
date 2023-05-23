namespace AspectCore.DynamicProxy
{
    public class AspectInvalidOperationException : AspectInvocationException
    {
        public AspectInvalidOperationException(AspectContext aspectContext, string message) : base(aspectContext, message) { }
    }
}