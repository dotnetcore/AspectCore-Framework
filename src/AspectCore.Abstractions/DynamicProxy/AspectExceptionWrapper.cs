using System;
using System.Runtime.ExceptionServices;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public class AspectExceptionWrapper : IAspectExceptionWrapper
    {
        private readonly IAspectConfiguration _configuration;
        private ExceptionDispatchInfo _exceptionInfo;

        public AspectExceptionWrapper(IAspectConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Wrap(AspectContext aspectContext, Exception exception)
        {
            if (!_configuration.ThrowAspectException || exception is AspectInvocationException _)
            {
                _exceptionInfo = ExceptionDispatchInfo.Capture(exception);
                return;
            }

            _exceptionInfo = ExceptionDispatchInfo.Capture(new AspectInvocationException(aspectContext, exception));
        }

        public void ThrowIfFailed()
        {
            _exceptionInfo?.Throw();
        }
    }
}