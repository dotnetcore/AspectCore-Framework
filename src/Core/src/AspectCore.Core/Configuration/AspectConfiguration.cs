using AspectCore.Injector;

namespace AspectCore.Configuration
{
    public sealed class AspectConfiguration : IAspectConfiguration
    {
        public AspectValidationHandlerCollection ValidationHandlers { get; }

        public InterceptorCollection Interceptors { get; }

        public NonAspectPredicateCollection NonAspectPredicates { get; }

        public IServiceContainer ServiceContainer { get; }

        public AspectConfiguration()
            : this(null)
        {
        }

        public AspectConfiguration(IServiceContainer serviceContainer)
        {
            ServiceContainer = serviceContainer ?? new ServiceContainer();
            ValidationHandlers = new AspectValidationHandlerCollection().AddDefault(this);
            Interceptors = new InterceptorCollection();
            NonAspectPredicates = new NonAspectPredicateCollection().AddDefault();
        }
    }
}