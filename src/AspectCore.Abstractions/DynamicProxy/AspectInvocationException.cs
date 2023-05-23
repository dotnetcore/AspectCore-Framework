using System;

namespace AspectCore.DynamicProxy
{
    public class AspectInvocationException : Exception
    {
        public AspectContext AspectContext { get; }

        public AspectInvocationException(AspectContext aspectContext, string message) : this(aspectContext, message, null) { }

        public AspectInvocationException(AspectContext aspectContext, Exception innerException)
            : this(aspectContext, $"Exception has been thrown by the aspect of an invocation. ---> {innerException?.Message}.", innerException) { }

        public AspectInvocationException(AspectContext aspectContext, string message, Exception innerException) : base(message, innerException)
        {
            AspectContext = aspectContext;
        }
    }
}