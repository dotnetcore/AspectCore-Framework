namespace AspectCore.DynamicProxy
{
    public class AspectInvalidCastException : AspectInvocationException
    {
        public AspectInvalidCastException(AspectContext aspectContext, string message) : base(aspectContext, message) { }
    }
}