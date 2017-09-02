using AspectCore.Injector;

namespace AspectCore.Configuration
{
    public interface IAspectConfiguration
    {
        AspectValidationHandlerCollection ValidationHandlers { get; }

        InterceptorCollection Interceptors { get; }

        NonAspectPredicateCollection NonAspectPredicates { get; }

        IServiceContainer ServiceContainer { get; }
    }
}