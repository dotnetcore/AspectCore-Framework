using System;

namespace AspectCore.DynamicProxy
{
    public interface IAspectExceptionWrapper
    {
        Exception Wrap(AspectContext aspectContext, Exception exception);
    }
}