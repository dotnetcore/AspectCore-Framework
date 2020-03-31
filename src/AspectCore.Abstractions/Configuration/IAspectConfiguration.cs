using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;

namespace AspectCore.Configuration
{
    [NonAspect]
    public interface IAspectConfiguration
    {
        AspectValidationHandlerCollection ValidationHandlers { get; }

        InterceptorCollection Interceptors { get; }

        NonAspectPredicateCollection NonAspectPredicates { get; }

        bool ThrowAspectException { get; set; }
    }
}