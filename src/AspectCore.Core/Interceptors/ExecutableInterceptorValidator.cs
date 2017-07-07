using System;
using System.Linq;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public class ExecutableInterceptorValidator : IExecutableInterceptorValidator
    {
        private readonly AspectScopeManager _aspectScopeManager;
        private readonly IInterceptorProvider _interceptorProvider;

        public ExecutableInterceptorValidator(AspectScopeManager aspectScopeManager, IInterceptorProvider interceptorProvider)
        {
            _aspectScopeManager = aspectScopeManager ?? throw new ArgumentNullException(nameof(aspectScopeManager));
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
        }

        public bool CanExecute(Abstractions.AspectContext context, IExecutableInterceptor interceptor)
        {
            if (interceptor.Execution == ExecutionMode.PerExecuted || _aspectScopeManager.Count <= 1)
            {
                return true;
            }
            AspectScope scope;
            if (!_aspectScopeManager.TryGetScope(context, out scope) || scope.Level == 0)
            {
                return true;
            }
            var scopes = _aspectScopeManager.GetScopes().Where(x => x.ScopeId != scope.ScopeId && x.Level < scope.Level).OrderBy(x => x.Level).ToArray();
            if (interceptor.Execution == ExecutionMode.PerNested)
            {
                var preContext = scopes[scopes.Length - 1].AspectContext;
                return CanExecuteInternal(preContext, interceptor);
            }
            return scopes.Select(x => x.AspectContext).Any(x => CanExecuteInternal(x, interceptor));
        }

        private bool CanExecuteInternal(AspectContext context, IExecutableInterceptor interceptor) =>
               _interceptorProvider.GetInterceptors(context.Target.ServiceMethod).Where(x => x.GetType() == interceptor.GetType()).Any(x => CanExecute(context, x));
    }
}