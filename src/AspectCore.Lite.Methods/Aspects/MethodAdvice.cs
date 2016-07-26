using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Methods.Aspects
{
    public abstract class MethodAdvice : IAdvice
    {
        public Task Execute(AspectContext aspectContext)
        {
            if (aspectContext == null) throw new ArgumentNullException(nameof(aspectContext));
            if (!(aspectContext is MethodAspectContext)) throw new ArgumentException("Invalid AspectContext.", nameof(aspectContext));
            return MethodExecute((MethodAspectContext)aspectContext);
        }

        protected virtual Task MethodExecute(MethodAspectContext aspectContext)
        {
            return aspectContext.Next(aspectContext);
        }
    }
}
