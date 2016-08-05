using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Methods.Aspects
{
    public abstract class MethodAdvice : IAdvice
    {
        public Task ExecuteAsync(AspectContext aspectContext)
        {
            if (aspectContext == null)
                throw new ArgumentNullException(nameof(aspectContext));

            if (!(aspectContext is MethodAspectContext))
                throw new InvalidCastException("Invalid AspectContext Type.");

            return MethodExecuteAsync((MethodAspectContext)aspectContext);
        }

        protected abstract Task MethodExecuteAsync(MethodAspectContext aspectContext);
    }
}
