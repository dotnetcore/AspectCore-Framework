using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core.Configuration
{
    public sealed class DelegateInterceptorFactory : InterceptorFactory
    {
        private readonly Func<AspectDelegate, AspectDelegate> _aspectDelegate;
        private readonly int _order;

        public DelegateInterceptorFactory(Func<AspectDelegate, AspectDelegate> aspectDelegate, int order, params AspectPredicate[] predicates)
            : base(predicates)
        {
            _aspectDelegate = aspectDelegate ?? throw new ArgumentNullException(nameof(aspectDelegate));
            _order = order;
        }

        public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
        {
            return new DelegateInterceptor(_aspectDelegate, _order);
        }
    }
}
