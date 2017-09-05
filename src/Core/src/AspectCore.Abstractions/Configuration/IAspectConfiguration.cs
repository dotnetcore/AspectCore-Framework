using AspectCore.DynamicProxy;
using AspectCore.Injector;

namespace AspectCore.Configuration
{
    [NonAspect]
    public interface IAspectConfiguration
    {
        AspectValidationHandlerCollection ValidationHandlers { get; }

        InterceptorCollection Interceptors { get; }

        NonAspectPredicateCollection NonAspectPredicates { get; }

        IServiceContainer ServiceContainer { get; }
    }
}