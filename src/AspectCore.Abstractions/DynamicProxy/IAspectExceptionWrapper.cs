using System;

namespace AspectCore.DynamicProxy
{
    public interface IAspectExceptionWrapper
    {
        void Wrap(AspectContext aspectContext, Exception exception);

        void ThrowIfFailed();
    }
}