namespace AspectCore.Abstractions
{
    public interface IAspectConfiguration
    {
        AspectValidationHandlerCollection ValidationHandlers { get; }

        InterceptorCollection Interceptors { get; }

        NonAspectPredicateCollection NonAspectPredicates { get; }

        IServiceContainer ServiceContainer { get; }
    }
}