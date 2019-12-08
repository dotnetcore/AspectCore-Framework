using System;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public class AspectExceptionWrapper : IAspectExceptionWrapper
    {
        private readonly IAspectConfiguration _configuration;

        public AspectExceptionWrapper(IAspectConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Exception Wrap(AspectContext aspectContext, Exception exception)
        {
            if (!_configuration.ThrowAspectException)
            {
                return exception;
            }

            if (exception is AspectInvocationException aspectInvocationException)
            {
                return aspectInvocationException;
            }

            return new AspectInvocationException(aspectContext, exception);
        }
    }
}