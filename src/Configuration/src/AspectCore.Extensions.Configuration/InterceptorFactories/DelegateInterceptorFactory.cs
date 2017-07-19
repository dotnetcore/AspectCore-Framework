using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.Configuration.InterceptorFactories
{
    public sealed class DelegateInterceptorFactory : InterceptorFactory
    {
        private readonly Func<AspectDelegate, AspectDelegate> _aspectDelegate;
        private readonly int _order;

        public DelegateInterceptorFactory(Func<AspectDelegate, AspectDelegate> aspectDelegate, int order = 0)
        {
            _aspectDelegate = aspectDelegate ?? throw new ArgumentNullException(nameof(aspectDelegate));
            _order = order;
        }

        public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
